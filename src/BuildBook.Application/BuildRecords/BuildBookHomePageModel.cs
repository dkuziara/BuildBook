namespace BuildBook.Application.BuildRecords;

public sealed record BuildBookHomePageModel(
    IReadOnlyList<HomePageRecordSummary> RecentlyViewedRecords,
    IReadOnlyList<HomePageRecordSummary> RecentlyUpdatedRecords);
