using BuildBook.Application.BuildRecords;

namespace BuildBook.Tests;

public class BuildRegisterTests
{
    [Fact]
    public void RowContainsDefaultRegisterColumns()
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

        Assert.Equal(1, row.Id);
        Assert.Equal("CDM61100", row.ProductCode);
        Assert.Equal("RadSight Access Terminal", row.ProductName);
        Assert.Equal("1000000", row.SerialNumber);
        Assert.Equal("APVL", row.CustomerName);
        Assert.Equal("RADSIGHT-11996", row.MachineName);
        Assert.Equal("1.3.6", row.RadSightVersion);
        Assert.Equal("Windows 10", row.WindowsVersion);
        Assert.Equal(new DateOnly(2026, 6, 20), row.DateAssembled);
        Assert.Equal(new DateOnly(2026, 6, 24), row.DateShipped);
        Assert.Equal("QA Team", row.CheckedBy);
    }

    [Fact]
    public void FilterReportsWhetherAnyFilterIsSet()
    {
        var emptyFilter = new BuildRegisterFilter();
        var customerIdFilter = new BuildRegisterFilter { CustomerId = 42 };
        var customerFilter = new BuildRegisterFilter { Customer = "APVL" };
        var dateFilter = new BuildRegisterFilter { DateShipped = new DateOnly(2026, 6, 24) };

        Assert.False(emptyFilter.HasAnyFilter());
        Assert.True(customerIdFilter.HasAnyFilter());
        Assert.True(customerFilter.HasAnyFilter());
        Assert.True(dateFilter.HasAnyFilter());
    }

    [Fact]
    public void FilterDefaultsToLastUpdatedDescending()
    {
        var filter = new BuildRegisterFilter();

        Assert.Equal(BuildRegisterSortColumn.LastUpdated, filter.SortBy);
        Assert.True(filter.SortDescending);
    }
}
