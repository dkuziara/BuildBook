using BuildBook.Application.BuildRecords;
using BuildBook.Infrastructure.Persistence.BuildRecords;

namespace BuildBook.Tests;

public class BuildRegisterCsvExporterTests
{
    [Fact]
    public async Task ExportAsync_WritesNonSensitiveRegisterColumnsAsCsv()
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
                "Device, Test",
                "1000001",
                "North \"Lab\"",
                null,
                null,
                null,
                null,
                null,
                null,
                new DateTimeOffset(2026, 6, 25, 8, 30, 0, TimeSpan.Zero))
        ]);
        var exporter = new BuildRegisterCsvExporter(reader);

        var csv = await exporter.ExportAsync(new BuildRegisterFilter { Customer = "APVL" });

        Assert.Contains("Product code,Product name,Serial number,Customer,Machine name,RadSight version,Windows version,Date assembled,Date shipped,Checked by,Last updated", csv);
        Assert.Contains("CDM61100,RadSight Access Terminal,1000000,APVL,RADSIGHT-11996,1.3.6,Windows 10,2026-06-20,2026-06-24,QA Team,", csv);
        Assert.Contains("CDM61101,\"Device, Test\",1000001,\"North \"\"Lab\"\"\",,,,,,,", csv);
        Assert.DoesNotContain("Password", csv);
        Assert.DoesNotContain("BitLocker", csv);
        Assert.DoesNotContain("RecoveryKey", csv);
        Assert.Equal("APVL", reader.LastFilter?.Customer);
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
