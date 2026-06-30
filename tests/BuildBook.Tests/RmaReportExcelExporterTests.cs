using System.IO.Compression;
using System.Text;
using BuildBook.Application.Rmas;
using BuildBook.Domain.Rmas;
using BuildBook.Infrastructure.Persistence.Rmas;

namespace BuildBook.Tests;

public class RmaReportExcelExporterTests
{
    [Fact]
    public async Task ExportAsync_WritesNonSensitiveRmaReportColumnsAsExcelWorkbook()
    {
        var reader = new StubRmaReportReader(
        [
            new RmaReportRow(
                1,
                "RMA-0001",
                RmaStatus.ReadyToShip,
                "Acme Medical",
                "Device <Test>",
                "CDM61100",
                "SN-1000",
                "Boot failure",
                RmaFaultCategory.HardwareFailure,
                RmaRootCauseCategory.ComponentFailure,
                RmaWarrantyStatus.OutOfWarranty,
                true,
                true,
                true,
                "PO-100",
                null,
                125.50m,
                149.75m,
                "North & South",
                new DateOnly(2026, 6, 29),
                null,
                new DateOnly(2026, 6, 20),
                new DateOnly(2026, 6, 24),
                new DateOnly(2026, 6, 30),
                new DateTimeOffset(2026, 6, 19, 8, 0, 0, TimeSpan.Zero),
                new DateTimeOffset(2026, 6, 25, 14, 0, 0, TimeSpan.Zero),
                null,
                1,
                new DateTimeOffset(2026, 6, 24, 9, 0, 0, TimeSpan.Zero),
                10,
                5,
                2,
                4,
                1)
        ]);
        var exporter = new RmaReportExcelExporter(reader);

        var workbook = await exporter.ExportAsync(new RmaReportFilter { Scope = RmaReportScope.ChargeableRepairs });

        using var archive = new ZipArchive(new MemoryStream(workbook), ZipArchiveMode.Read);
        var worksheetXml = ReadEntryText(archive, "xl/worksheets/sheet1.xml");
        var workbookXml = ReadEntryText(archive, "xl/workbook.xml");

        Assert.Contains("RMA Reports", workbookXml);
        Assert.Contains("RMA number", worksheetXml);
        Assert.Contains("Device &lt;Test&gt;", worksheetXml);
        Assert.Contains("North &amp; South", worksheetXml);
        Assert.DoesNotContain("Password", worksheetXml);
        Assert.DoesNotContain("BitLocker", worksheetXml);
        Assert.DoesNotContain("RecoveryKey", worksheetXml);
        Assert.Equal(RmaReportScope.ChargeableRepairs, reader.LastFilter?.Scope);
    }

    private static string ReadEntryText(ZipArchive archive, string entryPath)
    {
        using var stream = archive.GetEntry(entryPath)!.Open();
        using var reader = new StreamReader(stream, Encoding.UTF8);
        return reader.ReadToEnd();
    }

    private sealed class StubRmaReportReader(IReadOnlyList<RmaReportRow> rows) : IRmaReportReader
    {
        public RmaReportFilter? LastFilter { get; private set; }

        public Task<IReadOnlyList<RmaReportRow>> ListAsync(
            RmaReportFilter? filter = null,
            CancellationToken cancellationToken = default)
        {
            LastFilter = filter;
            return Task.FromResult(rows);
        }

        public Task<RmaDurationMetrics?> GetMetricsAsync(int rmaRecordId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<RmaDurationMetrics?>(null);
        }
    }
}
