using System.Collections.Concurrent;
using BuildBook.Application.BuildRecords;

namespace BuildBook.Infrastructure.Persistence.BuildRecords;

public sealed class RecentlyViewedBuildRecordTracker : IRecentlyViewedBuildRecordTracker
{
    private const int MaxTrackedRecordsPerUser = 10;
    private readonly ConcurrentDictionary<string, List<RecentBuildRecordView>> recentViewsByUser =
        new(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyList<RecentBuildRecordView> ListRecentViews(string? userIdentity, int maxCount = 5)
    {
        if (string.IsNullOrWhiteSpace(userIdentity))
        {
            return [];
        }

        if (!recentViewsByUser.TryGetValue(userIdentity.Trim(), out var recentViews))
        {
            return [];
        }

        lock (recentViews)
        {
            return recentViews
                .Take(Math.Max(0, maxCount))
                .ToArray();
        }
    }

    public void TrackView(int buildRecordId, string? userIdentity)
    {
        if (string.IsNullOrWhiteSpace(userIdentity))
        {
            return;
        }

        var recentViews = recentViewsByUser.GetOrAdd(userIdentity.Trim(), _ => []);

        lock (recentViews)
        {
            recentViews.RemoveAll(view => view.BuildRecordId == buildRecordId);
            recentViews.Insert(0, new RecentBuildRecordView(buildRecordId, DateTimeOffset.UtcNow));

            if (recentViews.Count > MaxTrackedRecordsPerUser)
            {
                recentViews.RemoveRange(MaxTrackedRecordsPerUser, recentViews.Count - MaxTrackedRecordsPerUser);
            }
        }
    }
}
