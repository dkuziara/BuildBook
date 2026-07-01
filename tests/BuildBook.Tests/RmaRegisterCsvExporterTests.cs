using BuildBook.Application.Rmas;
using BuildBook.Domain.Rmas;
using BuildBook.Infrastructure.Persistence.Rmas;

namespace BuildBook.Tests;

public class RmaRegisterCsvExporterTests
{
    [Fact]
    public async Task ExportAsync_WritesRmaRegisterColumnsAsCsv()
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
                "North \"Lab\"",
                "Device, Test",
                null,
                "Pending inspection",
                null,
                null,
                null,
                false,
                null,
                new DateTimeOffset(2026, 6, 29, 8, 30, 0, TimeSpan.Zero))
        ]);
        var exporter = new RmaRegisterCsvExporter(reader);

        var csv = await exporter.ExportAsync(new RmaRegisterFilter { Customer = "Acme" });

        Assert.Contains("RMA number,Status,Customer,Product,Serial,Fault summary,Priority,Assigned to,Due date,Build Record,Last updated", csv);
        Assert.Contains("RMA-0001,Work In Progress,Acme Medical,RadSight Access Terminal,SN-1000,Boot failure,High,Giles,5 Jul 2026,Linked,", csv);
        Assert.Contains("RMA-0002,Booked In,\"North \"\"Lab\"\"\",\"Device, Test\",Not recorded,Pending inspection,Not set,Not recorded,Not recorded,Unlinked,", csv);
        Assert.DoesNotContain("Password", csv);
        Assert.DoesNotContain("BitLocker", csv);
        Assert.Equal("Acme", reader.LastFilter?.Customer);
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
