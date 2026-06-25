using BuildBook.Application.BuildRecords;
using Microsoft.EntityFrameworkCore;

namespace BuildBook.Infrastructure.Persistence.BuildRecords;

public sealed class HomePageReader(
    IDbContextFactory<BuildBookDbContext> dbContextFactory,
    IRecentlyViewedBuildRecordTracker recentlyViewedBuildRecordTracker) : IHomePageReader
{
    private const int MaxRecentRecords = 5;

    public async Task<BuildBookHomePageModel> GetAsync(
        string? userIdentity,
        CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        var recentlyUpdatedRecords = await dbContext.BuildRecords
            .AsNoTracking()
            .Where(buildRecord => buildRecord.IsActive)
            .OrderByDescending(buildRecord => buildRecord.LastUpdatedAt)
            .ThenBy(buildRecord => buildRecord.SerialNumber)
            .Select(buildRecord => new HomePageRecordSummary(
                buildRecord.Id,
                buildRecord.ProductCode,
                buildRecord.ProductName,
                buildRecord.SerialNumber,
                buildRecord.Customer == null ? null : buildRecord.Customer.Name,
                buildRecord.MachineName,
                buildRecord.LastUpdatedAt))
            .Take(MaxRecentRecords)
            .ToListAsync(cancellationToken);

        var recentViews = recentlyViewedBuildRecordTracker.ListRecentViews(userIdentity, MaxRecentRecords);
        var recentlyViewedRecords = await LoadRecentlyViewedRecordsAsync(dbContext, recentViews, cancellationToken);

        return new BuildBookHomePageModel(recentlyViewedRecords, recentlyUpdatedRecords);
    }

    private static async Task<IReadOnlyList<HomePageRecordSummary>> LoadRecentlyViewedRecordsAsync(
        BuildBookDbContext dbContext,
        IReadOnlyList<RecentBuildRecordView> recentViews,
        CancellationToken cancellationToken)
    {
        if (recentViews.Count == 0)
        {
            return [];
        }

        var buildRecordIds = recentViews
            .Select(view => view.BuildRecordId)
            .Distinct()
            .ToArray();

        var buildRecordsById = await dbContext.BuildRecords
            .AsNoTracking()
            .Where(buildRecord => buildRecord.IsActive && buildRecordIds.Contains(buildRecord.Id))
            .Select(buildRecord => new
            {
                buildRecord.Id,
                buildRecord.ProductCode,
                buildRecord.ProductName,
                buildRecord.SerialNumber,
                CustomerName = buildRecord.Customer == null ? null : buildRecord.Customer.Name,
                buildRecord.MachineName
            })
            .ToDictionaryAsync(buildRecord => buildRecord.Id, cancellationToken);

        return recentViews
            .Where(view => buildRecordsById.ContainsKey(view.BuildRecordId))
            .Select(view =>
            {
                var buildRecord = buildRecordsById[view.BuildRecordId];

                return new HomePageRecordSummary(
                    buildRecord.Id,
                    buildRecord.ProductCode,
                    buildRecord.ProductName,
                    buildRecord.SerialNumber,
                    buildRecord.CustomerName,
                    buildRecord.MachineName,
                    view.ViewedAt);
            })
            .ToArray();
    }
}
