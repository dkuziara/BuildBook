using BuildBook.Application.BuildRecords;
using BuildBook.Infrastructure.Persistence.BuildRecords;

namespace BuildBook.Tests;

public class HomePageTests
{
    [Fact]
    public void SummaryContainsExpectedDisplayFields()
    {
        var summary = new HomePageRecordSummary(
            1,
            "CDM61100",
            "RadSight Access Terminal",
            "1000000",
            "APVL",
            "RADSIGHT-11996",
            new DateTimeOffset(2026, 6, 25, 9, 30, 0, TimeSpan.Zero));

        Assert.Equal(1, summary.Id);
        Assert.Equal("CDM61100", summary.ProductCode);
        Assert.Equal("RadSight Access Terminal", summary.ProductName);
        Assert.Equal("1000000", summary.SerialNumber);
        Assert.Equal("APVL", summary.CustomerName);
        Assert.Equal("RADSIGHT-11996", summary.MachineName);
        Assert.Equal(new DateTimeOffset(2026, 6, 25, 9, 30, 0, TimeSpan.Zero), summary.ActivityAt);
    }

    [Fact]
    public void RecentlyViewedTrackerMovesLatestViewToFrontAndLimitsHistory()
    {
        var tracker = new RecentlyViewedBuildRecordTracker();

        tracker.TrackView(1, "DOMAIN\\alice");
        tracker.TrackView(2, "DOMAIN\\alice");
        tracker.TrackView(3, "DOMAIN\\alice");
        tracker.TrackView(2, "DOMAIN\\alice");
        tracker.TrackView(4, "DOMAIN\\alice");
        tracker.TrackView(5, "DOMAIN\\alice");
        tracker.TrackView(6, "DOMAIN\\alice");

        var recentViews = tracker.ListRecentViews("DOMAIN\\alice");

        Assert.Equal([6, 5, 4, 2, 3], recentViews.Select(view => view.BuildRecordId).ToArray());
    }

    [Fact]
    public void HomePageReaderDoesNotReferenceSensitiveFields()
    {
        var readerPath = Path.Combine(
            GetRepositoryRoot(),
            "src",
            "BuildBook.Infrastructure",
            "Persistence",
            "BuildRecords",
            "HomePageReader.cs");
        var readerContent = File.ReadAllText(readerPath);

        Assert.DoesNotContain("BuildRecordSecret", readerContent);
        Assert.DoesNotContain("Password", readerContent);
        Assert.DoesNotContain("BitLocker", readerContent);
        Assert.DoesNotContain("RecoveryKey", readerContent);
    }

    private static string GetRepositoryRoot()
    {
        return Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
    }
}
