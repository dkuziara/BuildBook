using BuildBook.Application.Rmas;
using BuildBook.Domain.Rmas;
using BuildBook.Infrastructure.Persistence.Rmas;

namespace BuildBook.Tests;

public class RmaReportCsvExporterTests
{
    [Fact]
    public async Task ExportAsync_WritesNonSensitiveRmaReportColumnsAsCsv()
    {
        var reader = new StubRmaReportReader(
        [
            new RmaReportRow(
                1,
                "RMA-0001",
                RmaStatus.WorkInProgress,
                "Acme Medical",
                "RadSight Access Terminal",
                "CDM61100",
                "SN-1000",
                "Boot failure",
                RmaFaultCategory.HardwareFailure,
                RmaRootCauseCategory.ComponentFailure,
                RmaWarrantyStatus.OutOfWarranty,
                true,
                true,
                false,
                null,
                null,
                125.50m,
                149.75m,
                "Giles",
                new DateOnly(2026, 6, 29),
                "Waiting for parts",
                new DateOnly(2026, 6, 20),
                new DateOnly(2026, 6, 24),
                null,
                new DateTimeOffset(2026, 6, 19, 8, 0, 0, TimeSpan.Zero),
                new DateTimeOffset(2026, 6, 25, 14, 0, 0, TimeSpan.Zero),
                null,
                1,
                new DateTimeOffset(2026, 6, 24, 9, 0, 0, TimeSpan.Zero),
                10,
                5,
                2,
                4,
                null),
            new RmaReportRow(
                2,
                "RMA-0002",
                RmaStatus.Shipped,
                "North \"Lab\"",
                "Device, Test",
                null,
                "SN-2000",
                "Test fault",
                null,
                null,
                null,
                false,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                new DateOnly(2026, 6, 28),
                new DateTimeOffset(2026, 6, 21, 8, 0, 0, TimeSpan.Zero),
                new DateTimeOffset(2026, 6, 28, 9, 30, 0, TimeSpan.Zero),
                null,
                0,
                new DateTimeOffset(2026, 6, 26, 8, 0, 0, TimeSpan.Zero),
                7,
                2,
                0,
                null,
                1)
        ]);
        var exporter = new RmaReportCsvExporter(reader);

        var csv = await exporter.ExportAsync(new RmaReportFilter { Scope = RmaReportScope.AwaitingApproval });

        Assert.Contains("RMA number,Status,Customer,Product code,Product name,Serial number,Fault summary", csv);
        Assert.Contains("RMA-0001,WorkInProgress,Acme Medical,CDM61100,RadSight Access Terminal,SN-1000,Boot failure", csv);
        Assert.Contains("RMA-0002,Shipped,\"North \"\"Lab\"\"\",,\"Device, Test\",SN-2000,Test fault", csv);
        Assert.DoesNotContain("Password", csv);
        Assert.DoesNotContain("BitLocker", csv);
        Assert.DoesNotContain("RecoveryKey", csv);
        Assert.Equal(RmaReportScope.AwaitingApproval, reader.LastFilter?.Scope);
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
