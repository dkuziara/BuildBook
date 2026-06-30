using BuildBook.Application.BuildRecords;
using BuildBook.Infrastructure.Persistence.BuildRecords;

namespace BuildBook.Tests;

public class MissingDataReportCsvExporterTests
{
    [Fact]
    public async Task ExportAsync_WritesSelectedMissingDataRowsAsCsv()
    {
        var reader = new StubMissingDataReportReader(
        [
            new MissingDataReportRow(
                1,
                "CDM61100",
                "RadSight Access Terminal",
                "1000000",
                null,
                "RADSIGHT-11996",
                "1.3.6",
                "Windows 10",
                new DateOnly(2026, 6, 24),
                new DateTimeOffset(2026, 6, 24, 12, 0, 0, TimeSpan.Zero),
                true,
                false,
                false),
            new MissingDataReportRow(
                2,
                "CDM61101",
                "Device, Test",
                "1000001",
                "North \"Lab\"",
                null,
                null,
                null,
                null,
                new DateTimeOffset(2026, 6, 25, 8, 30, 0, TimeSpan.Zero),
                false,
                true,
                true)
        ]);
        var exporter = new MissingDataReportCsvExporter(reader);

        var csv = await exporter.ExportAsync(MissingDataReportType.Customer);

        Assert.Contains("Product code,Product name,Serial number,Customer,Machine name,RadSight version,Windows version,Date shipped,Last updated", csv);
        Assert.Contains("CDM61100,RadSight Access Terminal,1000000,,RADSIGHT-11996,1.3.6,Windows 10,2026-06-24,", csv);
        Assert.DoesNotContain("1000001", csv);
        Assert.DoesNotContain("Password", csv);
        Assert.DoesNotContain("BitLocker", csv);
    }

    private sealed class StubMissingDataReportReader(IReadOnlyList<MissingDataReportRow> rows) : IMissingDataReportReader
    {
        public Task<IReadOnlyList<MissingDataReportRow>> ListActiveAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(rows);
        }
    }
}
