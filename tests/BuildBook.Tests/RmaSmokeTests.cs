using System.Diagnostics;
using System.Net;
using System.Text;
using BuildBook.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Playwright;

namespace BuildBook.Tests;

public class RmaSmokeTests
{
    [Fact]
    public async Task RmaRegister_CreateOpenEditChecklistAndStatus_SmokePath()
    {
        await using var harness = await RmaSmokeTestHarness.StartAsync();

        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Channel = harness.BrowserChannel,
            Headless = true
        });

        var page = await browser.NewPageAsync();
        await page.GotoAsync($"{harness.BaseUrl}/rmas", new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle
        });

        Assert.Contains("RMA Register", await page.Locator("body").InnerTextAsync(), StringComparison.Ordinal);

        await page.GetByRole(AriaRole.Link, new() { Name = "Add RMA" }).ClickAsync();
        await page.WaitForURLAsync("**/rmas/new");

        await page.GetByLabel("Customer").FillAsync(harness.CustomerName);
        await page.GetByLabel("Product name").FillAsync(harness.ProductName);
        await page.GetByLabel("Product code").FillAsync(harness.ProductCode);
        await page.GetByLabel("Serial number").FillAsync(harness.SerialNumber);
        await page.GetByLabel("Fault summary").FillAsync("No power");
        await page.GetByLabel("Fault description").FillAsync("Unit does not power on.");
        await page.GetByLabel("Migration source").FillAsync("Planner manual recreation");
        await page.GetByLabel("Original Planner task title").FillAsync(harness.PlannerTaskTitle);
        await page.GetByLabel("Original Planner notes").FillAsync(harness.PlannerNotes);
        await page.GetByRole(AriaRole.Button, new() { Name = "Create RMA" }).ClickAsync();

        await page.WaitForURLAsync("**/rmas/*");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var createdBody = await page.Locator("body").InnerTextAsync();
        Assert.Contains(harness.PlannerTaskTitle, createdBody, StringComparison.Ordinal);
        Assert.Contains(harness.PlannerNotes, createdBody, StringComparison.Ordinal);

        var intakeSection = page.Locator("section[aria-labelledby='rma-intake-heading']");
        await intakeSection.GetByRole(AriaRole.Button, new() { Name = "Edit" }).ClickAsync();
        await page.GetByLabel("Support ticket number").FillAsync("SMOKE-TICKET-1");
        await page.GetByLabel("Migration source").FillAsync("Planner cutover batch");
        await page.GetByRole(AriaRole.Button, new() { Name = "Save intake details" }).ClickAsync();

        Assert.True(
            await WaitForBodyTextAsync(page, "RMA intake details were saved.", TimeSpan.FromSeconds(10)),
            "The intake save confirmation did not appear.");

        var checklistSection = page.Locator("section[aria-labelledby='rma-checklist-heading']");
        await checklistSection.GetByRole(AriaRole.Button, new() { Name = "Mark complete" }).First.ClickAsync();

        Assert.True(
            await WaitForBodyTextAsync(page, "Checklist item was updated.", TimeSpan.FromSeconds(10)),
            "The checklist update confirmation did not appear.");

        var statusSection = page.Locator("section[aria-labelledby='rma-status-heading']");
        await statusSection.GetByRole(AriaRole.Button, new() { Name = "Change status" }).ClickAsync();
        await page.Locator("#edit-rma-status").SelectOptionAsync(new[] { "WorkInProgress" });
        await page.GetByRole(AriaRole.Button, new() { Name = "Save status change" }).ClickAsync();

        Assert.True(
            await WaitForBodyTextAsync(page, "RMA status was updated.", TimeSpan.FromSeconds(10)),
            "The status change confirmation did not appear.");

        var finalBody = await page.Locator("body").InnerTextAsync();
        Assert.Contains("Work In Progress", finalBody, StringComparison.Ordinal);
        Assert.Contains("Planner cutover batch", finalBody, StringComparison.Ordinal);
        Assert.Contains("SMOKE-TICKET-1", finalBody, StringComparison.Ordinal);
    }

    private sealed class RmaSmokeTestHarness : IAsyncDisposable
    {
        private readonly Process process;
        private readonly StringBuilder processLog;
        private readonly Task outputPump;
        private readonly Task errorPump;
        private readonly string connectionString;

        private RmaSmokeTestHarness(
            Process process,
            StringBuilder processLog,
            Task outputPump,
            Task errorPump,
            string connectionString,
            string keyDirectory,
            string baseUrl,
            string browserChannel,
            string customerName,
            string productName,
            string productCode,
            string serialNumber,
            string plannerTaskTitle,
            string plannerNotes)
        {
            this.process = process;
            this.processLog = processLog;
            this.outputPump = outputPump;
            this.errorPump = errorPump;
            this.connectionString = connectionString;
            KeyDirectory = keyDirectory;
            BaseUrl = baseUrl;
            BrowserChannel = browserChannel;
            CustomerName = customerName;
            ProductName = productName;
            ProductCode = productCode;
            SerialNumber = serialNumber;
            PlannerTaskTitle = plannerTaskTitle;
            PlannerNotes = plannerNotes;
        }

        public string BaseUrl { get; }

        public string BrowserChannel { get; }

        public string CustomerName { get; }

        public string ProductName { get; }

        public string ProductCode { get; }

        public string SerialNumber { get; }

        public string PlannerTaskTitle { get; }

        public string PlannerNotes { get; }

        private string KeyDirectory { get; }

        public static async Task<RmaSmokeTestHarness> StartAsync(string developmentRole = "Administrator")
        {
            var testId = Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
            var customerName = $"Smoke RMA Customer {testId}";
            var productName = $"Smoke RMA Device {testId}";
            var productCode = $"RMA-PC-{testId}";
            var serialNumber = $"RMA-SN-{testId}";
            var plannerTaskTitle = $"Planner task {testId}";
            var plannerNotes = $"Original planner notes {testId}";
            var databaseName = $"BuildBook_RmaSmoke_{testId}";
            var baseUrl = $"http://127.0.0.1:{GetFreePort()}";
            var browserChannel = Environment.GetEnvironmentVariable("BUILDBOOK_SMOKE_BROWSER_CHANNEL") ?? "msedge";
            var connectionString = BuildConnectionString(databaseName);
            var keyDirectory = Path.Combine(Path.GetTempPath(), $"buildbook-rma-smoke-keys-{testId}");
            Directory.CreateDirectory(keyDirectory);

            await CreateDatabaseAsync(connectionString);

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
            startInfo.Environment["BuildBook__Authorization__DevelopmentRole"] = developmentRole;
            startInfo.Environment["BuildBook__EnableDetailedErrors"] = "true";
            startInfo.Environment["BuildBook__DataProtectionKeyDirectory"] = keyDirectory;

            var process = Process.Start(startInfo)
                ?? throw new InvalidOperationException("BuildBook smoke test could not start the web application process.");

            var processLog = new StringBuilder();
            var harness = new RmaSmokeTestHarness(
                process,
                processLog,
                PumpAsync(process.StandardOutput, processLog),
                PumpAsync(process.StandardError, processLog),
                connectionString,
                keyDirectory,
                baseUrl,
                browserChannel,
                customerName,
                productName,
                productCode,
                serialNumber,
                plannerTaskTitle,
                plannerNotes);

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
            }

            try
            {
                await Task.WhenAll(outputPump, errorPump);
            }
            catch
            {
            }

            await DeleteDatabaseAsync(connectionString);

            try
            {
                Directory.Delete(KeyDirectory, recursive: true);
            }
            catch
            {
            }
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
                    using var response = await client.GetAsync($"{BaseUrl}/rmas", timeout.Token);
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        return;
                    }
                }
                catch
                {
                }

                await Task.Delay(500, timeout.Token);
            }

            throw new TimeoutException(
                $"BuildBook smoke test app did not become ready within the timeout.{Environment.NewLine}{processLog}");
        }

        private static async Task CreateDatabaseAsync(string connectionString)
        {
            var options = new DbContextOptionsBuilder<BuildBookDbContext>()
                .UseSqlServer(connectionString)
                .Options;

            await using var dbContext = new BuildBookDbContext(options);
            await dbContext.Database.MigrateAsync();
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
