using BuildBook.Application.BuildRecords;
using BuildBook.Infrastructure.Persistence.BuildRecords;

namespace BuildBook.Tests;

public class BuildRegisterExportProjectionTests
{
    [Fact]
    public void ProjectionUsesExpectedNonSensitiveColumnsOnly()
    {
        Assert.Equal(
            [
                "Product code",
                "Product name",
                "Serial number",
                "Customer",
                "Machine name",
                "RadSight version",
                "Windows version",
                "Date assembled",
                "Date shipped",
                "Checked by",
                "Last updated"
            ],
            BuildRegisterExportProjection.Headers);
    }

    [Fact]
    public void ProjectionMatchesHeaderCount()
    {
        var row = new BuildRegisterRow(
            1,
            "CDM61100",
            "RadSight Access Terminal",
            "1000000",
            "APVL",
            "RADSIGHT-11996",
            "1.3.6",
            "Windows 10",
            new DateOnly(2026, 6, 20),
            new DateOnly(2026, 6, 24),
            "QA Team",
            new DateTimeOffset(2026, 6, 24, 12, 0, 0, TimeSpan.Zero));

        var values = BuildRegisterExportProjection.Project(row);

        Assert.Equal(BuildRegisterExportProjection.Headers.Count, values.Length);
    }
}
