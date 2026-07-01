using System.IO.Compression;
using System.Text;
using BuildBook.Application.Rmas;
using BuildBook.Domain.Rmas;
using BuildBook.Infrastructure.Persistence.Rmas;

namespace BuildBook.Tests;

public class RmaRegisterExcelExporterTests
{
    [Fact]
    public async Task ExportAsync_WritesRmaRegisterColumnsAsExcelWorkbook()
    {
        var reader = new StubRmaRegisterReader(
        [
            new RmaRegisterRow(
                1,
                "RMA-0001",
                RmaStatus.WorkInProgress,
                "Acme Medical",
                "RadSight Access Terminal",
                "SN-1000",
                "Boot failure",
                RmaPriority.High,
                "Giles",
                new DateOnly(2026, 7, 5),
                true,
                22,
                new DateTimeOffset(2026, 6, 30, 12, 0, 0, TimeSpan.Zero)),
            new RmaRegisterRow(
                2,
                "RMA-0002",
                RmaStatus.BookedIn,
                "North & South",
                "Device <Test>",
                null,
                "Pending inspection",
                null,
                null,
                null,
                false,
                null,
                new DateTimeOffset(2026, 6, 29, 8, 30, 0, TimeSpan.Zero))
        ]);
        var exporter = new RmaRegisterExcelExporter(reader);

        var workbook = await exporter.ExportAsync(new RmaRegisterFilter { Status = RmaStatus.WorkInProgress });

        using var archive = new ZipArchive(new MemoryStream(workbook), ZipArchiveMode.Read);
        var worksheetXml = ReadEntryText(archive, "xl/worksheets/sheet1.xml");
        var workbookXml = ReadEntryText(archive, "xl/workbook.xml");

        Assert.Contains("RMA Register", workbookXml);
        Assert.Contains("RMA number", worksheetXml);
        Assert.Contains("RadSight Access Terminal", worksheetXml);
        Assert.Contains("Device &lt;Test&gt;", worksheetXml);
        Assert.Contains("North &amp; South", worksheetXml);
        Assert.DoesNotContain("Password", worksheetXml);
        Assert.DoesNotContain("BitLocker", worksheetXml);
        Assert.Equal(RmaStatus.WorkInProgress, reader.LastFilter?.Status);
    }

    private static string ReadEntryText(ZipArchive archive, string entryPath)
    {
        using var stream = archive.GetEntry(entryPath)!.Open();
        using var reader = new StreamReader(stream, Encoding.UTF8);
        return reader.ReadToEnd();
    }

    private sealed class StubRmaRegisterReader(IReadOnlyList<RmaRegisterRow> rows) : IRmaRegisterReader
    {
        public RmaRegisterFilter? LastFilter { get; private set; }

        public Task<IReadOnlyList<RmaRegisterRow>> ListAsync(
            RmaRegisterFilter? filter = null,
            CancellationToken cancellationToken = default)
        {
            LastFilter = filter;
            return Task.FromResult(rows);
        }
    }
}
