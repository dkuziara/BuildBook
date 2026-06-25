using System.IO.Compression;
using System.Text;
using BuildBook.Domain.BuildRecords;
using BuildBook.Infrastructure.Persistence;
using BuildBook.Infrastructure.Persistence.BuildRecords;
using Microsoft.EntityFrameworkCore;

namespace BuildBook.Tests;

public class SpreadsheetImportMappingServiceTests
{
    [Fact]
    public async Task BuildReviewAsync_ReadsCsvHeadersAndSuggestsRequiredMappings()
    {
        var service = new SpreadsheetImportMappingService();
        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes("Product Code,Product Name,Serial Number,Wi-Fi Password\r\n"));

        var review = await service.BuildReviewAsync("buildbook-import.csv", stream);

        Assert.Equal(4, review.ColumnMappings.Count);
        Assert.Equal("ProductCode", review.ColumnMappings[0].SuggestedFieldKey);
        Assert.Equal("ProductName", review.ColumnMappings[1].SuggestedFieldKey);
        Assert.Equal("SerialNumber", review.ColumnMappings[2].SuggestedFieldKey);
        Assert.Equal("WifiPassword", review.ColumnMappings[3].SuggestedFieldKey);
        Assert.Contains(review.AvailableFields, field => field.Key == "ProductCode" && field.IsRequired);
        Assert.Contains(review.AvailableFields, field => field.Key == "WifiPassword" && field.IsSensitive);
        Assert.Empty(review.Notices);
    }

    [Fact]
    public async Task BuildReviewAsync_ReadsXlsxHeadersFromFirstWorksheet()
    {
        var service = new SpreadsheetImportMappingService();
        await using var stream = CreateXlsxStream("Product Code", "Machine Name", "BitLocker Recovery Key");

        var review = await service.BuildReviewAsync("buildbook-import.xlsx", stream);

        Assert.Equal(["Product Code", "Machine Name", "BitLocker Recovery Key"], review.ColumnMappings.Select(mapping => mapping.SourceColumnName).ToArray());
        Assert.Equal("ProductCode", review.ColumnMappings[0].SuggestedFieldKey);
        Assert.Equal("MachineName", review.ColumnMappings[1].SuggestedFieldKey);
        Assert.Equal("BitLockerRecoveryKey", review.ColumnMappings[2].SuggestedFieldKey);
        Assert.Empty(review.Notices);
    }

    [Fact]
    public async Task BuildReviewAsync_AddsNoticeForLegacyXlsFiles()
    {
        var service = new SpreadsheetImportMappingService();
        await using var stream = new MemoryStream([1, 2, 3]);

        var review = await service.BuildReviewAsync("legacy-import.xls", stream);

        Assert.Empty(review.ColumnMappings);
        Assert.Contains(review.Notices, notice => notice.Contains(".xlsx and .csv", StringComparison.Ordinal));
    }

    [Fact]
    public async Task BuildPreviewAsync_ReadsCsvRowsAndMasksSensitiveValues()
    {
        var service = new SpreadsheetImportMappingService();
        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(
            "Product Code,Serial Number,Wi-Fi Password\r\nCDM61100,1000000,super-secret\r\nCDM61101,1000001,\r\n"));

        var preview = await service.BuildPreviewAsync(
            "buildbook-import.csv",
            stream,
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["Product Code"] = "ProductCode",
                ["Serial Number"] = "SerialNumber",
                ["Wi-Fi Password"] = "WifiPassword"
            });

        Assert.Equal(2, preview.RowsRead);
        Assert.Equal(2, preview.RowsShown);
        Assert.Equal(3, preview.Columns.Count);
        Assert.Equal("CDM61100", preview.Rows[0].Values["ProductCode"]);
        Assert.Equal("1000000", preview.Rows[0].Values["SerialNumber"]);
        Assert.Equal("************", preview.Rows[0].Values["WifiPassword"]);
        Assert.Equal(string.Empty, preview.Rows[1].Values["WifiPassword"]);
        Assert.Contains(preview.Notices, notice => notice.Contains("masked in the preview", StringComparison.Ordinal));
    }

    [Fact]
    public async Task BuildPreviewAsync_ReadsXlsxRowsAndLimitsPreviewLength()
    {
        var service = new SpreadsheetImportMappingService();
        await using var stream = CreateXlsxStream(
            ["Product Code", "Machine Name"],
            [["CDM61100", "RADSIGHT-11996"], ["CDM61101", "RADSIGHT-11997"], ["CDM61102", "RADSIGHT-11998"], ["CDM61103", "RADSIGHT-11999"], ["CDM61104", "RADSIGHT-12000"], ["CDM61105", "RADSIGHT-12001"], ["CDM61106", "RADSIGHT-12002"], ["CDM61107", "RADSIGHT-12003"], ["CDM61108", "RADSIGHT-12004"], ["CDM61109", "RADSIGHT-12005"], ["CDM61110", "RADSIGHT-12006"]]);

        var preview = await service.BuildPreviewAsync(
            "buildbook-import.xlsx",
            stream,
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["Product Code"] = "ProductCode",
                ["Machine Name"] = "MachineName"
            });

        Assert.Equal(11, preview.RowsRead);
        Assert.Equal(10, preview.RowsShown);
        Assert.Equal("CDM61100", preview.Rows[0].Values["ProductCode"]);
        Assert.Equal("RADSIGHT-11996", preview.Rows[0].Values["MachineName"]);
        Assert.Contains(preview.Notices, notice => notice.Contains("Showing the first 10 rows of 11 data rows.", StringComparison.Ordinal));
    }

    [Fact]
    public async Task BuildValidationAsync_FlagsRequiredFieldsDuplicateSerialsBadDatesAndIncompleteRows()
    {
        var service = new SpreadsheetImportMappingService();
        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(
            "Product Code,Product Name,Serial Number,Date Assembled,Date Shipped,Machine Name\r\n" +
            "CDM61100,RadSight Access Terminal,1000000,2026-06-20,2026-06-21,RADSIGHT-11996\r\n" +
            ",Missing Product Code,1000001,2026-06-20,2026-06-21,RADSIGHT-11997\r\n" +
            "CDM61102,Duplicate Serial,1000000,2026-06-22,2026-06-23,RADSIGHT-11998\r\n" +
            "CDM61103,Bad Dates,1000003,not-a-date,2026-06-19,RADSIGHT-11999\r\n" +
            "CDM61104,Ship Before Assemble,1000004,2026-06-22,2026-06-21,RADSIGHT-12000\r\n" +
            "CDM61105,,,2026-06-22,,\r\n"));

        var validation = await service.BuildValidationAsync(
            "buildbook-import.csv",
            stream,
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["Product Code"] = "ProductCode",
                ["Product Name"] = "ProductName",
                ["Serial Number"] = "SerialNumber",
                ["Date Assembled"] = "DateAssembled",
                ["Date Shipped"] = "DateShipped",
                ["Machine Name"] = "MachineName"
            });

        Assert.Equal(6, validation.RowsRead);
        Assert.True(validation.ErrorCount >= 7);
        Assert.True(validation.WarningCount >= 1);
        Assert.Contains(validation.Issues, issue => issue.SourceRowNumber == 3 && issue.Message == "Product code is required.");
        Assert.Contains(validation.Issues, issue => issue.SourceRowNumber == 2 && issue.Message == "Duplicate serial number '1000000'.");
        Assert.Contains(validation.Issues, issue => issue.SourceRowNumber == 5 && issue.Message == "Date assembled is not a valid date.");
        Assert.Contains(validation.Issues, issue => issue.SourceRowNumber == 6 && issue.Message == "Date shipped cannot be earlier than date assembled.");
        Assert.Contains(validation.Issues, issue => issue.SourceRowNumber == 7 && issue.Message == "Row appears incomplete.");
    }

    [Fact]
    public async Task BuildValidationAsync_ReturnsNoIssuesForValidRows()
    {
        var service = new SpreadsheetImportMappingService();
        await using var stream = CreateXlsxStream(
            ["Product Code", "Product Name", "Serial Number", "Date Assembled", "Date Shipped"],
            [["CDM61100", "RadSight Access Terminal", "1000000", "2026-06-20", "2026-06-21"]]);

        var validation = await service.BuildValidationAsync(
            "buildbook-import.xlsx",
            stream,
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["Product Code"] = "ProductCode",
                ["Product Name"] = "ProductName",
                ["Serial Number"] = "SerialNumber",
                ["Date Assembled"] = "DateAssembled",
                ["Date Shipped"] = "DateShipped"
            });

        Assert.Equal(1, validation.RowsRead);
        Assert.Equal(0, validation.ErrorCount);
        Assert.Equal(0, validation.WarningCount);
        Assert.Empty(validation.Issues);
    }

    [Fact]
    public async Task BuildImportAsync_CreatesBuildRecordsCustomersImportBatchAndAuditEntries()
    {
        await using var harness = await ImportHarness.CreateAsync();
        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(
            "Product Code,Product Name,Serial Number,Customer,Machine Name,Date Assembled,Wi-Fi Password\r\n" +
            "CDM61100,RadSight Access Terminal,1000000,APVL,RADSIGHT-11996,2026-06-20,super-secret\r\n" +
            "CDM61101,RadSight Access Terminal,1000001,APVL,RADSIGHT-11997,2026-06-21,another-secret\r\n"));

        var result = await harness.Service.BuildImportAsync(
            "buildbook-import.csv",
            stream,
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["Product Code"] = "ProductCode",
                ["Product Name"] = "ProductName",
                ["Serial Number"] = "SerialNumber",
                ["Customer"] = "Customer",
                ["Machine Name"] = "MachineName",
                ["Date Assembled"] = "DateAssembled",
                ["Wi-Fi Password"] = "WifiPassword"
            },
            "DOMAIN\\importer");

        Assert.NotNull(result.ImportBatchId);
        Assert.Equal(2, result.RowsRead);
        Assert.Equal(2, result.RecordsCreated);
        Assert.Equal(0, result.RecordsSkipped);
        Assert.Equal(0, result.WarningCount);
        Assert.Equal(0, result.ErrorCount);

        await using var verifyContext = harness.CreateContext();
        var buildRecords = await verifyContext.BuildRecords.Include(record => record.Customer).OrderBy(record => record.SerialNumber).ToListAsync();
        var importBatch = await verifyContext.Imports.SingleAsync();
        var importAudit = await verifyContext.BuildRecordAudit.SingleAsync(entry => entry.Action == AuditAction.ImportPerformed);

        Assert.Equal(2, buildRecords.Count);
        Assert.Equal("APVL", buildRecords[0].Customer?.Name);
        Assert.Equal(new DateOnly(2026, 6, 20), buildRecords[0].DateAssembled);
        Assert.Equal(2, await verifyContext.BuildRecordAudit.CountAsync(entry => entry.Action == AuditAction.Created));
        Assert.Empty(await verifyContext.BuildRecordSecrets.ToListAsync());
        Assert.Equal(2, importBatch.RecordsCreated);
        Assert.Equal(0, importBatch.RecordsSkipped);
        Assert.Equal(0, importBatch.WarningCount);
        Assert.Equal(ImportStatus.Completed, importBatch.Status);
        Assert.Equal("Spreadsheet import", importAudit.FieldChanged);
    }

    [Fact]
    public async Task BuildImportAsync_SkipsExistingSerialNumbersAndMarksWarnings()
    {
        await using var harness = await ImportHarness.CreateAsync(seedExistingRecord: true);
        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(
            "Product Code,Product Name,Serial Number,Customer\r\n" +
            "CDM61100,RadSight Access Terminal,EXISTING-1000000,APVL\r\n" +
            "CDM61101,RadSight Access Terminal,1000001,APVL\r\n"));

        var result = await harness.Service.BuildImportAsync(
            "buildbook-import.csv",
            stream,
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["Product Code"] = "ProductCode",
                ["Product Name"] = "ProductName",
                ["Serial Number"] = "SerialNumber",
                ["Customer"] = "Customer"
            },
            "DOMAIN\\importer");

        Assert.Equal(2, result.RowsRead);
        Assert.Equal(1, result.RecordsCreated);
        Assert.Equal(1, result.RecordsSkipped);
        Assert.Equal(1, result.WarningCount);
        Assert.Equal(0, result.ErrorCount);

        await using var verifyContext = harness.CreateContext();
        var importBatch = await verifyContext.Imports.SingleAsync();

        Assert.Equal(2, await verifyContext.BuildRecords.CountAsync());
        Assert.Equal(ImportStatus.CompletedWithWarnings, importBatch.Status);
    }

    private static MemoryStream CreateXlsxStream(IReadOnlyList<string> headers, IReadOnlyList<IReadOnlyList<string>>? rows = null)
    {
        var stream = new MemoryStream();

        using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: true))
        {
            WriteEntry(
                archive,
                "[Content_Types].xml",
                """
                <?xml version="1.0" encoding="UTF-8"?>
                <Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types">
                  <Default Extension="rels" ContentType="application/vnd.openxmlformats-package.relationships+xml"/>
                  <Default Extension="xml" ContentType="application/xml"/>
                  <Override PartName="/xl/workbook.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml"/>
                  <Override PartName="/xl/worksheets/sheet1.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml"/>
                  <Override PartName="/xl/sharedStrings.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.sharedStrings+xml"/>
                </Types>
                """);
            WriteEntry(
                archive,
                "_rels/.rels",
                """
                <?xml version="1.0" encoding="UTF-8"?>
                <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
                  <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument" Target="xl/workbook.xml"/>
                </Relationships>
                """);
            WriteEntry(
                archive,
                "xl/workbook.xml",
                """
                <?xml version="1.0" encoding="UTF-8"?>
                <workbook xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main" xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships">
                  <sheets>
                    <sheet name="Sheet1" sheetId="1" r:id="rId1"/>
                  </sheets>
                </workbook>
                """);
            WriteEntry(
                archive,
                "xl/_rels/workbook.xml.rels",
                """
                <?xml version="1.0" encoding="UTF-8"?>
                <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
                  <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet" Target="worksheets/sheet1.xml"/>
                </Relationships>
                """);
            WriteEntry(
                archive,
                "xl/sharedStrings.xml",
                BuildSharedStringsXml(headers, rows ?? []));
            WriteEntry(
                archive,
                "xl/worksheets/sheet1.xml",
                BuildWorksheetXml(headers.Count, rows ?? []));
        }

        stream.Position = 0;
        return stream;
    }

    private static MemoryStream CreateXlsxStream(params string[] headers)
    {
        return CreateXlsxStream(headers, []);
    }

    private static void WriteEntry(ZipArchive archive, string path, string contents)
    {
        var entry = archive.CreateEntry(path);
        using var writer = new StreamWriter(entry.Open(), Encoding.UTF8);
        writer.Write(contents);
    }

    private static string BuildSharedStringsXml(IReadOnlyList<string> headers, IReadOnlyList<IReadOnlyList<string>> rows)
    {
        var builder = new StringBuilder();
        builder.Append("""<?xml version="1.0" encoding="UTF-8"?><sst xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main">""");

        foreach (var header in headers)
        {
            builder.Append("<si><t>");
            builder.Append(System.Security.SecurityElement.Escape(header));
            builder.Append("</t></si>");
        }

        foreach (var row in rows)
        {
            foreach (var value in row)
            {
                builder.Append("<si><t>");
                builder.Append(System.Security.SecurityElement.Escape(value));
                builder.Append("</t></si>");
            }
        }

        builder.Append("</sst>");
        return builder.ToString();
    }

    private static string BuildWorksheetXml(int headerCount, IReadOnlyList<IReadOnlyList<string>> rows)
    {
        var builder = new StringBuilder();
        builder.Append("""<?xml version="1.0" encoding="UTF-8"?><worksheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main"><sheetData><row r="1">""");

        for (var index = 0; index < headerCount; index++)
        {
            builder.Append($"""<c r="{ToColumnLetter(index + 1)}1" t="s"><v>{index}</v></c>""");
        }

        builder.Append("</row>");

        var sharedStringIndex = headerCount;
        for (var rowIndex = 0; rowIndex < rows.Count; rowIndex++)
        {
            builder.Append($"""<row r="{rowIndex + 2}">""");

            for (var columnIndex = 0; columnIndex < rows[rowIndex].Count; columnIndex++)
            {
                builder.Append($"""<c r="{ToColumnLetter(columnIndex + 1)}{rowIndex + 2}" t="s"><v>{sharedStringIndex}</v></c>""");
                sharedStringIndex++;
            }

            builder.Append("</row>");
        }

        builder.Append("</sheetData></worksheet>");
        return builder.ToString();
    }

    private static string ToColumnLetter(int columnNumber)
    {
        var letters = new StringBuilder();

        while (columnNumber > 0)
        {
            columnNumber--;
            letters.Insert(0, (char)('A' + (columnNumber % 26)));
            columnNumber /= 26;
        }

        return letters.ToString();
    }

    private sealed class ImportHarness : IAsyncDisposable
    {
        private ImportHarness(DbContextOptions<BuildBookDbContext> options, SpreadsheetImportMappingService service)
        {
            Options = options;
            Service = service;
        }

        public DbContextOptions<BuildBookDbContext> Options { get; }

        public SpreadsheetImportMappingService Service { get; }

        public static async Task<ImportHarness> CreateAsync(bool seedExistingRecord = false)
        {
            var databaseName = $"BuildBookImport_{Guid.NewGuid():N}";
            var connectionString = $"Server=(localdb)\\MSSQLLocalDB;Database={databaseName};Trusted_Connection=True;TrustServerCertificate=True";
            var options = new DbContextOptionsBuilder<BuildBookDbContext>()
                .UseSqlServer(connectionString)
                .Options;

            await using var setupContext = new BuildBookDbContext(options);
            await setupContext.Database.MigrateAsync();

            if (seedExistingRecord)
            {
                setupContext.BuildRecords.Add(new BuildRecord
                {
                    ProductCode = "SEEDED",
                    ProductName = "Seeded Device",
                    SerialNumber = "EXISTING-1000000",
                    CreatedBy = "seed",
                    LastUpdatedBy = "seed"
                });
                await setupContext.SaveChangesAsync();
            }

            var factory = new TestDbContextFactory(options);
            var service = new SpreadsheetImportMappingService(factory, new BuildRecordAuditService());
            return new ImportHarness(options, service);
        }

        public BuildBookDbContext CreateContext()
        {
            return new BuildBookDbContext(Options);
        }

        public async ValueTask DisposeAsync()
        {
            await using var context = CreateContext();
            await context.Database.EnsureDeletedAsync();
        }
    }

    private sealed class TestDbContextFactory(DbContextOptions<BuildBookDbContext> options) : IDbContextFactory<BuildBookDbContext>
    {
        public BuildBookDbContext CreateDbContext()
        {
            return new BuildBookDbContext(options);
        }

        public Task<BuildBookDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new BuildBookDbContext(options));
        }
    }
}
