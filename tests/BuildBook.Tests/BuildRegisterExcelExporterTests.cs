using System.IO.Compression;
using System.Text;
using BuildBook.Application.BuildRecords;
using BuildBook.Infrastructure.Persistence.BuildRecords;

namespace BuildBook.Tests;

public class BuildRegisterExcelExporterTests
{
    [Fact]
    public async Task ExportAsync_WritesNonSensitiveRegisterColumnsAsExcelWorkbook()
    {
        var reader = new StubBuildRegisterReader(
        [
            new BuildRegisterRow(
                1,
                "CDM61100",
                42,
                "RadSight Access Terminal",
                "1000000",
                "APVL",
                "RADSIGHT-11996",
                "1.3.6",
                "Windows 10",
                new DateOnly(2026, 6, 20),
                new DateOnly(2026, 6, 24),
                "QA Team",
                new DateTimeOffset(2026, 6, 24, 12, 0, 0, TimeSpan.Zero)),
            new BuildRegisterRow(
                2,
                "CDM61101",
                null,
                "Device <Test>",
                "1000001",
                "North & South",
                null,
                null,
                null,
                null,
                null,
                null,
                new DateTimeOffset(2026, 6, 25, 8, 30, 0, TimeSpan.Zero))
        ]);
        var exporter = new BuildRegisterExcelExporter(reader);

        var workbook = await exporter.ExportAsync(new BuildRegisterFilter { Customer = "APVL" });

        using var archive = new ZipArchive(new MemoryStream(workbook), ZipArchiveMode.Read);
        var worksheetXml = ReadEntryText(archive, "xl/worksheets/sheet1.xml");
        var workbookXml = ReadEntryText(archive, "xl/workbook.xml");

        Assert.Contains("Build Register", workbookXml);
        Assert.Contains("Product code", worksheetXml);
        Assert.Contains("RadSight Access Terminal", worksheetXml);
        Assert.Contains("Device &lt;Test&gt;", worksheetXml);
        Assert.Contains("North &amp; South", worksheetXml);
        Assert.DoesNotContain("Password", worksheetXml);
        Assert.DoesNotContain("BitLocker", worksheetXml);
        Assert.DoesNotContain("RecoveryKey", worksheetXml);
        Assert.Equal("APVL", reader.LastFilter?.Customer);
    }

    private static string ReadEntryText(ZipArchive archive, string entryPath)
    {
        using var stream = archive.GetEntry(entryPath)!.Open();
        using var reader = new StreamReader(stream, Encoding.UTF8);
        return reader.ReadToEnd();
    }

    private sealed class StubBuildRegisterReader(IReadOnlyList<BuildRegisterRow> rows) : IBuildRegisterReader
    {
        public BuildRegisterFilter? LastFilter { get; private set; }

        public Task<IReadOnlyList<BuildRegisterRow>> ListAsync(
            BuildRegisterFilter? filter = null,
            CancellationToken cancellationToken = default)
        {
            LastFilter = filter;
            return Task.FromResult(rows);
        }
    }
}
