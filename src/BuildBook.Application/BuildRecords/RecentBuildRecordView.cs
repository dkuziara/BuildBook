namespace BuildBook.Application.BuildRecords;

public sealed record RecentBuildRecordView(int BuildRecordId, DateTimeOffset ViewedAt);
