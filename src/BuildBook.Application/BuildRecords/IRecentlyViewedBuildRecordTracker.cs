namespace BuildBook.Application.BuildRecords;

public interface IRecentlyViewedBuildRecordTracker
{
    IReadOnlyList<RecentBuildRecordView> ListRecentViews(string? userIdentity, int maxCount = 5);

    void TrackView(int buildRecordId, string? userIdentity);
}
