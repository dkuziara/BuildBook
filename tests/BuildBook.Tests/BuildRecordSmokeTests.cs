using System.Diagnostics;
using System.Net;
using System.Text;
using BuildBook.Domain.BuildRecords;
using BuildBook.Domain.Customers;
using BuildBook.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Playwright;

namespace BuildBook.Tests;

public class BuildRecordSmokeTests
{
    [Fact]
    public async Task BuildRegister_OpenRecord_EditNotes_SmokePath_DoesNotExposeSecrets()
    {
        await using var harness = await BuildBookSmokeTestHarness.StartAsync();

        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Channel = harness.BrowserChannel,
            Headless = true
        });

        var page = await browser.NewPageAsync();
        await page.GotoAsync($"{harness.BaseUrl}/build-register", new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle
        });

        await page.GetByLabel("Product code").FillAsync(harness.ProductCode);
        await page.GetByRole(AriaRole.Button, new() { Name = "Apply filters" }).ClickAsync();

        var resultRow = page.Locator("tbody tr").Filter(new LocatorFilterOptions
        {
            HasText = harness.ProductCode
        });

        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        Assert.Equal(1, await resultRow.CountAsync());
        Assert.Contains(harness.SerialNumber, await resultRow.InnerTextAsync());
        Assert.DoesNotContain(harness.SecretPlainText, await page.Locator("table").InnerTextAsync(), StringComparison.Ordinal);

        await resultRow.GetByRole(AriaRole.Link, new() { Name = harness.SerialNumber }).ClickAsync();
        await page.WaitForURLAsync("**/build-records/*");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        Assert.DoesNotContain(harness.SecretPlainText, await page.Locator("body").InnerTextAsync(), StringComparison.Ordinal);

        var notesSection = page.Locator("section[aria-labelledby='notes-heading']");
        await notesSection.GetByRole(AriaRole.Button, new() { Name = "Edit" }).ClickAsync();
        await page.Locator("#edit-note").FillAsync(harness.UpdatedNote);
        await page.GetByRole(AriaRole.Button, new() { Name = "Save Notes" }).ClickAsync();

        var saveSucceeded = await WaitForBodyTextAsync(page, "Notes were saved.", TimeSpan.FromSeconds(10));
        var bodyTextAfterSave = await page.Locator("body").InnerTextAsync();
        Assert.True(
            saveSucceeded,
            $"Notes save did not report success.{Environment.NewLine}{bodyTextAfterSave}");

        await page.ReloadAsync(new PageReloadOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle
        });

        Assert.Contains(harness.UpdatedNote, await notesSection.InnerTextAsync(), StringComparison.Ordinal);
        Assert.DoesNotContain(harness.SecretPlainText, await page.Locator("body").InnerTextAsync(), StringComparison.Ordinal);
    }

    private sealed class BuildBookSmokeTestHarness : IAsyncDisposable
    {
        private readonly Process process;
        private readonly StringBuilder processLog;
        private readonly Task outputPump;
        private readonly Task errorPump;
        private readonly string connectionString;

        private BuildBookSmokeTestHarness(
            Process process,
            StringBuilder processLog,
            Task outputPump,
            Task errorPump,
            string connectionString,
            string baseUrl,
            string browserChannel,
            string productCode,
            string serialNumber,
            string secretPlainText,
            string updatedNote)
        {
            this.process = process;
            this.processLog = processLog;
            this.outputPump = outputPump;
            this.errorPump = errorPump;
            this.connectionString = connectionString;
            BaseUrl = baseUrl;
            BrowserChannel = browserChannel;
            ProductCode = productCode;
            SerialNumber = serialNumber;
            SecretPlainText = secretPlainText;
            UpdatedNote = updatedNote;
        }

        public string BaseUrl { get; }

        public string BrowserChannel { get; }

        public string ProductCode { get; }

        public string SerialNumber { get; }

        public string SecretPlainText { get; }

        public string UpdatedNote { get; }

        public static async Task<BuildBookSmokeTestHarness> StartAsync()
        {
            var testId = Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
            var productCode = $"BI029-{testId}";
            var serialNumber = $"SMOKE-{testId}";
            var initialNote = $"Initial smoke note {testId}";
            var updatedNote = $"Updated smoke note {testId}";
            var secretPlainText = $"router-secret-{testId}";
            var databaseName = $"BuildBook_Smoke_{testId}";
            var baseUrl = $"http://127.0.0.1:{GetFreePort()}";
            var browserChannel = Environment.GetEnvironmentVariable("BUILDBOOK_SMOKE_BROWSER_CHANNEL") ?? "msedge";
            var connectionString = BuildConnectionString(databaseName);

            await CreateDatabaseAsync(connectionString, productCode, serialNumber, initialNote, secretPlainText);

            var startInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $".\\bin\\Debug\\net10.0\\BuildBook.Web.dll --urls {baseUrl}",
                WorkingDirectory = GetWebProjectPath(),
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            startInfo.Environment["ASPNETCORE_ENVIRONMENT"] = "Development";
            startInfo.Environment["ConnectionStrings__BuildBookDatabase"] = connectionString;
            startInfo.Environment["BuildBook__SeedDevelopmentData"] = "false";
            startInfo.Environment["BuildBook__Authorization__UseDevelopmentAuthentication"] = "true";
            startInfo.Environment["BuildBook__Authorization__DevelopmentRole"] = "Administrator";
            startInfo.Environment["BuildBook__EnableDetailedErrors"] = "true";

            var process = Process.Start(startInfo)
                ?? throw new InvalidOperationException("BuildBook smoke test could not start the web application process.");

            var processLog = new StringBuilder();
            var harness = new BuildBookSmokeTestHarness(
                process,
                processLog,
                PumpAsync(process.StandardOutput, processLog),
                PumpAsync(process.StandardError, processLog),
                connectionString,
                baseUrl,
                browserChannel,
                productCode,
                serialNumber,
                secretPlainText,
                updatedNote);

            await harness.WaitForApplicationAsync();
            return harness;
        }

        public async ValueTask DisposeAsync()
        {
            try
            {
                if (!process.HasExited)
                {
                    process.Kill(entireProcessTree: true);
                }
            }
            catch
            {
                // Best-effort cleanup for the local smoke app process.
            }

            try
            {
                await Task.WhenAll(outputPump, errorPump);
            }
            catch
            {
                // Ignore log pump errors during shutdown.
            }

            await DeleteDatabaseAsync(connectionString);
        }

        private async Task WaitForApplicationAsync()
        {
            using var client = new HttpClient();
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(45));

            while (!timeout.IsCancellationRequested)
            {
                if (process.HasExited)
                {
                    throw new InvalidOperationException(
                        $"BuildBook smoke test app exited before it became ready.{Environment.NewLine}{processLog}");
                }

                try
                {
                    using var response = await client.GetAsync($"{BaseUrl}/build-register", timeout.Token);
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        return;
                    }
                }
                catch
                {
                    // Keep polling until the app is ready or the timeout expires.
                }

                await Task.Delay(500, timeout.Token);
            }

            throw new TimeoutException(
                $"BuildBook smoke test app did not become ready within the timeout.{Environment.NewLine}{processLog}");
        }

        private static async Task CreateDatabaseAsync(
            string connectionString,
            string productCode,
            string serialNumber,
            string initialNote,
            string secretPlainText)
        {
            var options = new DbContextOptionsBuilder<BuildBookDbContext>()
                .UseSqlServer(connectionString)
                .Options;

            await using var dbContext = new BuildBookDbContext(options);
            await dbContext.Database.MigrateAsync();

            var customer = new Customer
            {
                Name = "Smoke Test Customer",
                CreatedBy = "Smoke test",
                LastUpdatedBy = "Smoke test"
            };

            var buildRecord = new BuildRecord
            {
                ProductCode = productCode,
                ProductName = "Smoke Test Device",
                ProductClassification = "Test",
                SerialNumber = serialNumber,
                InternalStatus = InternalStatus.Checked,
                AssembledIn = "Test Bench",
                AssembledBy = "Smoke Runner",
                DateAssembled = new DateOnly(2026, 6, 25),
                HardwareManufacturer = "Charthouse",
                ManufacturerPartNumber = productCode,
                ManufacturerRevision = "Rev Smoke",
                ManufacturerSerialNumber = $"MFG-{serialNumber}",
                Customer = customer,
                CustomerOrder = $"CO-{serialNumber}",
                OANumber = $"OA-{serialNumber}",
                InvoiceNumber = $"INV-{serialNumber}",
                DateShipped = new DateOnly(2026, 6, 25),
                ShippingNotes = "Smoke test shipment.",
                PanelDeviceModel = "Smoke Panel",
                PanelDeviceSerial = $"PANEL-{serialNumber}",
                PanelFirmwareVersion = "1.0.0-smoke",
                MachineName = $"MACHINE-{serialNumber}",
                RadioSerialNumber = $"RAD-{serialNumber}",
                RouterUsed = "Smoke Router",
                HardwareNotes = "Smoke test hardware notes.",
                DiskImageVersion = "SmokeImage-1",
                RadSightVersion = "SmokeRad-1",
                WindowsVersion = "Windows 11",
                WindowsLatestPatch = "KB-SMOKE",
                BleuvioFirmwareVersion = "SmokeBleuvio-1",
                CharthouseIrdaFirmwareVersion = "SmokeIrda-1",
                RadioFirmware = "SmokeRadio-1",
                RadSightUserLogin = "smoke-user",
                KioskUser = "smoke-kiosk",
                WindowsAdminUser = "smoke-admin",
                WifiSsid = "SmokeWifi",
                PackingList = $"PL-{serialNumber}",
                CheckedBy = "Smoke QA",
                Note = initialNote,
                CreatedBy = "Smoke test",
                LastUpdatedBy = "Smoke test"
            };

            buildRecord.Secrets.Add(new BuildRecordSecret
            {
                SecretType = SecretType.RouterPassword,
                SecretValueEncrypted = Encoding.UTF8.GetBytes(secretPlainText),
                CreatedBy = "Smoke test",
                LastUpdatedBy = "Smoke test"
            });

            await dbContext.Customers.AddAsync(customer);
            await dbContext.BuildRecords.AddAsync(buildRecord);
            await dbContext.SaveChangesAsync();
        }

        private static async Task DeleteDatabaseAsync(string connectionString)
        {
            var options = new DbContextOptionsBuilder<BuildBookDbContext>()
                .UseSqlServer(connectionString)
                .Options;

            await using var dbContext = new BuildBookDbContext(options);
            await dbContext.Database.EnsureDeletedAsync();
        }

        private static string BuildConnectionString(string databaseName)
        {
            var server = Environment.GetEnvironmentVariable("BUILDBOOK_SMOKE_SQLSERVER")
                ?? "lpc:(local)\\SQLEXPRESS";

            return $"Server={server};Database={databaseName};Trusted_Connection=True;Encrypt=False;TrustServerCertificate=True;MultipleActiveResultSets=true";
        }

        private static int GetFreePort()
        {
            var listener = new System.Net.Sockets.TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            var port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            return port;
        }

        private static string GetWebProjectPath()
        {
            return Path.GetFullPath(Path.Combine(GetRepositoryRoot(), "src", "BuildBook.Web"));
        }

        private static string GetRepositoryRoot()
        {
            return Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
        }

        private static Task PumpAsync(StreamReader reader, StringBuilder processLog)
        {
            return Task.Run(async () =>
            {
                while (true)
                {
                    var line = await reader.ReadLineAsync();
                    if (line is null)
                    {
                        break;
                    }

                    lock (processLog)
                    {
                        processLog.AppendLine(line);
                    }
                }
            });
        }
    }

    private static async Task<bool> WaitForBodyTextAsync(IPage page, string expectedText, TimeSpan timeout)
    {
        var timeoutAt = DateTimeOffset.UtcNow.Add(timeout);

        while (DateTimeOffset.UtcNow < timeoutAt)
        {
            var bodyText = await page.Locator("body").InnerTextAsync();
            if (bodyText.Contains(expectedText, StringComparison.Ordinal))
            {
                return true;
            }

            await page.WaitForTimeoutAsync(250);
        }

        return false;
    }
}
