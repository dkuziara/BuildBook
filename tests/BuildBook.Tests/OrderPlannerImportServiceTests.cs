using System.IO.Compression;
using System.Text;
using BuildBook.Domain.Security;
using BuildBook.Infrastructure.Persistence.Orders;
using System.Reflection;

namespace BuildBook.Tests;

public class OrderPlannerImportServiceTests
{
    [Fact]
    public async Task BuildReviewAsync_PrefersConsolidatedDataAndReadsPlanMetadata()
    {
        var service = new OrderPlannerImportService();
        await using var stream = CreatePlannerExportStream(
            ("Plan",
            [
                "Plan ID", "Plan Name", "Export Date"
            ],
            [
                ["plan-001", "Demo Orders", "2026-07-01 09:30"]
            ]),
            ("Consolidated Data",
            [
                "Task ID", "Task Name", "Bucket", "Priority", "Assigned To", "Checklist Items", "Completed Checklist Items", "Notes"
            ],
            [
                ["TASK-100", "Prepare demo shipment", "Prepared for Shipping", "High", "Alex Mason", "Build; Pack", "Build", "Customer requested Friday dispatch"]
            ]));

        var review = await service.BuildReviewAsync("planner-orders.xlsx", stream);

        Assert.Equal("Consolidated Data", review.PreferredWorksheetName);
        Assert.Equal("plan-001", review.PlanId);
        Assert.Equal("Demo Orders", review.PlanName);
        Assert.Equal(1, review.RowsRead);
        Assert.Equal(1, review.RowsShown);
        Assert.Contains("Plan", review.WorksheetNames);
        Assert.Contains("Consolidated Data", review.WorksheetNames);
        Assert.Equal("TASK-100", review.Rows[0].Values["PlannerTaskId"]);
        Assert.Equal("Prepare demo shipment", review.Rows[0].Values["OrderTitle"]);
    }

    [Fact]
    public async Task BuildValidationAsync_FlagsDuplicateTaskIdsUnknownBucketsUnknownUsersAndMissingTaskNames()
    {
        var service = new OrderPlannerImportService();
        await using var stream = CreatePlannerExportStream(
            ("Consolidated Data",
            [
                "Task ID", "Task Name", "Bucket", "Assigned To", "Created Date"
            ],
            [
                ["TASK-100", "Pack unit", "Prepared for Shipping", "Alex Mason", "2026-07-01"],
                ["TASK-100", "", "Unexpected Bucket", "Unknown Person", "bad-date"]
            ]));

        var validation = await service.BuildValidationAsync("planner-orders.xlsx", stream);

        Assert.Equal(2, validation.RowsRead);
        Assert.True(validation.ErrorCount >= 3);
        Assert.True(validation.WarningCount >= 2);
        Assert.Contains(validation.Issues, issue => issue.Message == "Task name is required.");
        Assert.Contains(validation.Issues, issue => issue.Message == "Created date is not a valid date.");
        Assert.Contains(validation.Issues, issue => issue.Message.Contains("Duplicate Planner task ID 'TASK-100'", StringComparison.Ordinal));
        Assert.Contains(validation.Issues, issue => issue.Message.Contains("Bucket 'Unexpected Bucket'", StringComparison.Ordinal));
        Assert.Contains(validation.Issues, issue => issue.Message.Contains("Assigned user 'Unknown Person'", StringComparison.Ordinal));
    }

    [Fact]
    public async Task BuildReviewAsync_ReadsNonSeekableUploadedWorkbookStream()
    {
        var service = new OrderPlannerImportService();
        await using var workbookStream = CreatePlannerExportStream(
            ("Consolidated Data",
            [
                "Task ID", "Task Name", "Bucket"
            ],
            [
                ["TASK-200", "Ship replacement unit", "Ready for Collection"]
            ]));
        using var uploadStream = new NonSeekableReadStream(workbookStream.ToArray());

        var review = await service.BuildReviewAsync("planner-orders.xlsx", uploadStream);

        Assert.Equal(1, review.RowsRead);
        Assert.Equal("TASK-200", review.Rows[0].Values["PlannerTaskId"]);
        Assert.Equal("Ship replacement unit", review.Rows[0].Values["OrderTitle"]);
    }

    [Fact]
    public async Task BuildImportAsync_WithoutDatabaseServices_ThrowsInvalidOperationException()
    {
        var service = new OrderPlannerImportService();
        await using var stream = CreatePlannerExportStream(
            ("Consolidated Data",
            [
                "Task ID", "Task Name", "Bucket"
            ],
            [
                ["TASK-100", "Prepare demo shipment", "Prepared for Shipping"]
            ]));

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.BuildImportAsync("planner-orders.xlsx", stream, @"DOMAIN\importer"));

        Assert.Contains("database services", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void TryFindApplicationUser_MatchesApplicationUserDisplayName()
    {
        var method = typeof(OrderPlannerImportService).GetMethod(
            "TryFindApplicationUser",
            BindingFlags.NonPublic | BindingFlags.Static);

        Assert.NotNull(method);

        var knownUsers = new List<ApplicationUser>
        {
            new()
            {
                Id = 42,
                WindowsUserName = @"DOMAIN\asmith",
                DisplayName = "Alex Mason",
                EmailAddress = "alex.mason@example.com",
                IsActive = true
            }
        };

        var parameters = new object?[] { knownUsers, "Alex Mason", null };
        var matched = (bool)method!.Invoke(null, parameters)!;

        Assert.True(matched);
        var matchedUser = Assert.IsType<ApplicationUser>(parameters[2]);
        Assert.Equal(42, matchedUser.Id);
        Assert.Equal("Alex Mason", matchedUser.DisplayName);
    }

    private static MemoryStream CreatePlannerExportStream(params (string SheetName, IReadOnlyList<string> Headers, IReadOnlyList<IReadOnlyList<string>> Rows)[] sheets)
    {
        var stream = new MemoryStream();

        using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: true))
        {
            WriteEntry(
                archive,
                "[Content_Types].xml",
                BuildContentTypesXml(sheets.Length));
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
                BuildWorkbookXml(sheets));
            WriteEntry(
                archive,
                "xl/_rels/workbook.xml.rels",
                BuildWorkbookRelationshipsXml(sheets.Length));
            WriteEntry(
                archive,
                "xl/sharedStrings.xml",
                BuildSharedStringsXml(sheets));

            var sharedStringIndex = 0;
            for (var sheetIndex = 0; sheetIndex < sheets.Length; sheetIndex++)
            {
                var sheet = sheets[sheetIndex];
                WriteEntry(
                    archive,
                    $"xl/worksheets/sheet{sheetIndex + 1}.xml",
                    BuildWorksheetXml(sheet.Headers, sheet.Rows, ref sharedStringIndex));
            }
        }

        stream.Position = 0;
        return stream;
    }

    private static string BuildContentTypesXml(int sheetCount)
    {
        var builder = new StringBuilder();
        builder.Append("""
                       <?xml version="1.0" encoding="UTF-8"?>
                       <Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types">
                         <Default Extension="rels" ContentType="application/vnd.openxmlformats-package.relationships+xml"/>
                         <Default Extension="xml" ContentType="application/xml"/>
                         <Override PartName="/xl/workbook.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml"/>
                         <Override PartName="/xl/sharedStrings.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.sharedStrings+xml"/>
                       """);

        for (var sheetIndex = 0; sheetIndex < sheetCount; sheetIndex++)
        {
            builder.Append($"""<Override PartName="/xl/worksheets/sheet{sheetIndex + 1}.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml"/>""");
        }

        builder.Append("</Types>");
        return builder.ToString();
    }

    private static string BuildWorkbookXml((string SheetName, IReadOnlyList<string> Headers, IReadOnlyList<IReadOnlyList<string>> Rows)[] sheets)
    {
        var builder = new StringBuilder();
        builder.Append("""<?xml version="1.0" encoding="UTF-8"?><workbook xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main" xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships"><sheets>""");

        for (var sheetIndex = 0; sheetIndex < sheets.Length; sheetIndex++)
        {
            builder.Append($"""<sheet name="{System.Security.SecurityElement.Escape(sheets[sheetIndex].SheetName)}" sheetId="{sheetIndex + 1}" r:id="rId{sheetIndex + 1}"/>""");
        }

        builder.Append("</sheets></workbook>");
        return builder.ToString();
    }

    private static string BuildWorkbookRelationshipsXml(int sheetCount)
    {
        var builder = new StringBuilder();
        builder.Append("""<?xml version="1.0" encoding="UTF-8"?><Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">""");

        for (var sheetIndex = 0; sheetIndex < sheetCount; sheetIndex++)
        {
            builder.Append($"""<Relationship Id="rId{sheetIndex + 1}" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet" Target="worksheets/sheet{sheetIndex + 1}.xml"/>""");
        }

        builder.Append("</Relationships>");
        return builder.ToString();
    }

    private static string BuildSharedStringsXml((string SheetName, IReadOnlyList<string> Headers, IReadOnlyList<IReadOnlyList<string>> Rows)[] sheets)
    {
        var builder = new StringBuilder();
        builder.Append("""<?xml version="1.0" encoding="UTF-8"?><sst xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main">""");

        foreach (var sheet in sheets)
        {
            foreach (var header in sheet.Headers)
            {
                builder.Append("<si><t>");
                builder.Append(System.Security.SecurityElement.Escape(header));
                builder.Append("</t></si>");
            }

            foreach (var row in sheet.Rows)
            {
                foreach (var value in row)
                {
                    builder.Append("<si><t>");
                    builder.Append(System.Security.SecurityElement.Escape(value));
                    builder.Append("</t></si>");
                }
            }
        }

        builder.Append("</sst>");
        return builder.ToString();
    }

    private static string BuildWorksheetXml(IReadOnlyList<string> headers, IReadOnlyList<IReadOnlyList<string>> rows, ref int sharedStringIndex)
    {
        var builder = new StringBuilder();
        builder.Append("""<?xml version="1.0" encoding="UTF-8"?><worksheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main"><sheetData><row r="1">""");

        for (var columnIndex = 0; columnIndex < headers.Count; columnIndex++)
        {
            builder.Append($"""<c r="{ToColumnLetter(columnIndex + 1)}1" t="s"><v>{sharedStringIndex}</v></c>""");
            sharedStringIndex++;
        }

        builder.Append("</row>");

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

    private static void WriteEntry(ZipArchive archive, string path, string contents)
    {
        var entry = archive.CreateEntry(path);
        using var writer = new StreamWriter(entry.Open(), Encoding.UTF8);
        writer.Write(contents);
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

    private sealed class NonSeekableReadStream(byte[] contents) : Stream
    {
        private readonly MemoryStream innerStream = new(contents);

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public override long Length => innerStream.Length;

        public override long Position
        {
            get => innerStream.Position;
            set => throw new NotSupportedException();
        }

        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return innerStream.Read(buffer, offset, count);
        }

        public override int Read(Span<byte> buffer)
        {
            return innerStream.Read(buffer);
        }

        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            return innerStream.ReadAsync(buffer, cancellationToken);
        }

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return innerStream.ReadAsync(buffer, offset, count, cancellationToken);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                innerStream.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
