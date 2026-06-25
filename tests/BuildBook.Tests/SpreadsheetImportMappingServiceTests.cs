using System.IO.Compression;
using System.Text;
using BuildBook.Infrastructure.Persistence.BuildRecords;

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

    private static MemoryStream CreateXlsxStream(params string[] headers)
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
                BuildSharedStringsXml(headers));
            WriteEntry(
                archive,
                "xl/worksheets/sheet1.xml",
                BuildWorksheetXml(headers.Length));
        }

        stream.Position = 0;
        return stream;
    }

    private static void WriteEntry(ZipArchive archive, string path, string contents)
    {
        var entry = archive.CreateEntry(path);
        using var writer = new StreamWriter(entry.Open(), Encoding.UTF8);
        writer.Write(contents);
    }

    private static string BuildSharedStringsXml(IReadOnlyList<string> headers)
    {
        var builder = new StringBuilder();
        builder.Append("""<?xml version="1.0" encoding="UTF-8"?><sst xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main">""");

        foreach (var header in headers)
        {
            builder.Append("<si><t>");
            builder.Append(System.Security.SecurityElement.Escape(header));
            builder.Append("</t></si>");
        }

        builder.Append("</sst>");
        return builder.ToString();
    }

    private static string BuildWorksheetXml(int headerCount)
    {
        var builder = new StringBuilder();
        builder.Append("""<?xml version="1.0" encoding="UTF-8"?><worksheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main"><sheetData><row r="1">""");

        for (var index = 0; index < headerCount; index++)
        {
            builder.Append($"""<c r="{ToColumnLetter(index + 1)}1" t="s"><v>{index}</v></c>""");
        }

        builder.Append("</row></sheetData></worksheet>");
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
}
