using BuildBook.Application.BuildRecords;

namespace BuildBook.Tests;

public class BuildRecordSearchTests
{
    [Fact]
    public void ResultContainsSearchDisplayFields()
    {
        var result = new BuildRecordSearchResult(
            1,
            "CDM61100",
            "RadSight Access Terminal",
            "1000000",
            "APVL",
            "RADSIGHT-11996",
            "1.3.6",
            "Windows 10",
            new DateOnly(2026, 6, 24));

        Assert.Equal(1, result.Id);
        Assert.Equal("CDM61100", result.ProductCode);
        Assert.Equal("RadSight Access Terminal", result.ProductName);
        Assert.Equal("1000000", result.SerialNumber);
        Assert.Equal("APVL", result.CustomerName);
        Assert.Equal("RADSIGHT-11996", result.MachineName);
        Assert.Equal("1.3.6", result.RadSightVersion);
        Assert.Equal("Windows 10", result.WindowsVersion);
        Assert.Equal(new DateOnly(2026, 6, 24), result.DateShipped);
    }

    [Fact]
    public void SearchServiceDoesNotReferenceSensitiveFields()
    {
        var servicePath = Path.Combine(
            GetRepositoryRoot(),
            "src",
            "BuildBook.Infrastructure",
            "Persistence",
            "BuildRecords",
            "BuildRecordSearchService.cs");
        var serviceContent = File.ReadAllText(servicePath);

        Assert.DoesNotContain("BuildRecordSecret", serviceContent);
        Assert.DoesNotContain("Password", serviceContent);
        Assert.DoesNotContain("BitLocker", serviceContent);
        Assert.DoesNotContain("RecoveryKey", serviceContent);
    }

    private static string GetRepositoryRoot()
    {
        return Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
    }
}
