using System.IO.Compression;
using System.Text;
using BuildBook.Application.BuildRecords;
using BuildBook.Infrastructure.Persistence.BuildRecords;

namespace BuildBook.Tests;

public class MissingDataReportExcelExporterTests
{
    [Fact]
    public async Task ExportAsync_WritesSelectedMissingDataRowsAsExcelWorkbook()
    {
        var reader = new StubMissingDataReportReader(
        [
            new MissingDataReportRow(
                1,
                "CDM61100",
                "Device <Test>",
                "1000000",
                "North & South",
                "RADSIGHT-11996",
                "1.3.6",
                "Windows 10",
                null,
                new DateTimeOffset(2026, 6, 24, 12, 0, 0, TimeSpan.Zero),
                false,
                true,
                false)
        ]);
        var exporter = new MissingDataReportExcelExporter(reader);

        var workbook = await exporter.ExportAsync(MissingDataReportType.RecoveryData);

        using var archive = new ZipArchive(new MemoryStream(workbook), ZipArchiveMode.Read);
        var worksheetXml = ReadEntryText(archive, "xl/worksheets/sheet1.xml");
        var workbookXml = ReadEntryText(archive, "xl/workbook.xml");

        Assert.Contains("Missing Data Report", workbookXml);
        Assert.Contains("Product code", worksheetXml);
        Assert.Contains("Device &lt;Test&gt;", worksheetXml);
        Assert.Contains("North &amp; South", worksheetXml);
        Assert.DoesNotContain("Password", worksheetXml);
        Assert.DoesNotContain("BitLocker", worksheetXml);
    }

    private static string ReadEntryText(ZipArchive archive, string entryPath)
    {
        using var stream = archive.GetEntry(entryPath)!.Open();
        using var reader = new StreamReader(stream, Encoding.UTF8);
        return reader.ReadToEnd();
    }

    private sealed class StubMissingDataReportReader(IReadOnlyList<MissingDataReportRow> rows) : IMissingDataReportReader
    {
        public Task<IReadOnlyList<MissingDataReportRow>> ListActiveAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(rows);
        }
    }
}
