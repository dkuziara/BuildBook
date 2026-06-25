using BuildBook.Application.BuildRecords;
using Microsoft.EntityFrameworkCore;

namespace BuildBook.Infrastructure.Persistence.BuildRecords;

public sealed class BuildRegisterReader(
    IDbContextFactory<BuildBookDbContext> dbContextFactory) : IBuildRegisterReader
{
    public async Task<IReadOnlyList<BuildRegisterRow>> ListAsync(
        BuildRegisterFilter? filter = null,
        CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        var query = dbContext.BuildRecords
            .AsNoTracking()
            .Where(buildRecord => buildRecord.IsActive);

        if (filter is not null)
        {
            if (!string.IsNullOrWhiteSpace(filter.Customer))
            {
                var customerPattern = CreateLikePattern(filter.Customer);
                query = query.Where(buildRecord =>
                    buildRecord.Customer != null
                    && EF.Functions.Like(buildRecord.Customer.Name, customerPattern, @"\"));
            }

            if (!string.IsNullOrWhiteSpace(filter.ProductCode))
            {
                var productCodePattern = CreateLikePattern(filter.ProductCode);
                query = query.Where(buildRecord =>
                    EF.Functions.Like(buildRecord.ProductCode, productCodePattern, @"\"));
            }

            if (filter.DateShipped is not null)
            {
                query = query.Where(buildRecord => buildRecord.DateShipped == filter.DateShipped);
            }

            if (!string.IsNullOrWhiteSpace(filter.RadSightVersion))
            {
                var radSightVersionPattern = CreateLikePattern(filter.RadSightVersion);
                query = query.Where(buildRecord =>
                    EF.Functions.Like(buildRecord.RadSightVersion!, radSightVersionPattern, @"\"));
            }

            if (!string.IsNullOrWhiteSpace(filter.WindowsVersion))
            {
                var windowsVersionPattern = CreateLikePattern(filter.WindowsVersion);
                query = query.Where(buildRecord =>
                    EF.Functions.Like(buildRecord.WindowsVersion!, windowsVersionPattern, @"\"));
            }
        }

        query = ApplySorting(query, filter);

        return await query
            .Select(buildRecord => new BuildRegisterRow(
                buildRecord.Id,
                buildRecord.ProductCode,
                buildRecord.ProductName,
                buildRecord.SerialNumber,
                buildRecord.Customer == null ? null : buildRecord.Customer.Name,
                buildRecord.MachineName,
                buildRecord.RadSightVersion,
                buildRecord.WindowsVersion,
                buildRecord.DateAssembled,
                buildRecord.DateShipped,
                buildRecord.CheckedBy,
                buildRecord.LastUpdatedAt))
            .ToListAsync(cancellationToken);
    }

    private static IQueryable<Domain.BuildRecords.BuildRecord> ApplySorting(
        IQueryable<Domain.BuildRecords.BuildRecord> query,
        BuildRegisterFilter? filter)
    {
        var sortBy = filter?.SortBy ?? BuildRegisterSortColumn.LastUpdated;
        var sortDescending = filter?.SortDescending ?? true;

        IOrderedQueryable<Domain.BuildRecords.BuildRecord> orderedQuery = (sortBy, sortDescending) switch
        {
            (BuildRegisterSortColumn.ProductCode, false) => query.OrderBy(buildRecord => buildRecord.ProductCode),
            (BuildRegisterSortColumn.ProductCode, true) => query.OrderByDescending(buildRecord => buildRecord.ProductCode),
            (BuildRegisterSortColumn.ProductName, false) => query.OrderBy(buildRecord => buildRecord.ProductName),
            (BuildRegisterSortColumn.ProductName, true) => query.OrderByDescending(buildRecord => buildRecord.ProductName),
            (BuildRegisterSortColumn.SerialNumber, false) => query.OrderBy(buildRecord => buildRecord.SerialNumber),
            (BuildRegisterSortColumn.SerialNumber, true) => query.OrderByDescending(buildRecord => buildRecord.SerialNumber),
            (BuildRegisterSortColumn.Customer, false) => query.OrderBy(buildRecord => buildRecord.Customer == null ? null : buildRecord.Customer.Name),
            (BuildRegisterSortColumn.Customer, true) => query.OrderByDescending(buildRecord => buildRecord.Customer == null ? null : buildRecord.Customer.Name),
            (BuildRegisterSortColumn.MachineName, false) => query.OrderBy(buildRecord => buildRecord.MachineName),
            (BuildRegisterSortColumn.MachineName, true) => query.OrderByDescending(buildRecord => buildRecord.MachineName),
            (BuildRegisterSortColumn.RadSightVersion, false) => query.OrderBy(buildRecord => buildRecord.RadSightVersion),
            (BuildRegisterSortColumn.RadSightVersion, true) => query.OrderByDescending(buildRecord => buildRecord.RadSightVersion),
            (BuildRegisterSortColumn.WindowsVersion, false) => query.OrderBy(buildRecord => buildRecord.WindowsVersion),
            (BuildRegisterSortColumn.WindowsVersion, true) => query.OrderByDescending(buildRecord => buildRecord.WindowsVersion),
            (BuildRegisterSortColumn.DateAssembled, false) => query.OrderBy(buildRecord => buildRecord.DateAssembled),
            (BuildRegisterSortColumn.DateAssembled, true) => query.OrderByDescending(buildRecord => buildRecord.DateAssembled),
            (BuildRegisterSortColumn.DateShipped, false) => query.OrderBy(buildRecord => buildRecord.DateShipped),
            (BuildRegisterSortColumn.DateShipped, true) => query.OrderByDescending(buildRecord => buildRecord.DateShipped),
            (BuildRegisterSortColumn.CheckedBy, false) => query.OrderBy(buildRecord => buildRecord.CheckedBy),
            (BuildRegisterSortColumn.CheckedBy, true) => query.OrderByDescending(buildRecord => buildRecord.CheckedBy),
            (BuildRegisterSortColumn.LastUpdated, false) => query.OrderBy(buildRecord => buildRecord.LastUpdatedAt),
            _ => query.OrderByDescending(buildRecord => buildRecord.LastUpdatedAt)
        };

        return orderedQuery.ThenBy(buildRecord => buildRecord.SerialNumber);
    }

    private static string CreateLikePattern(string value)
    {
        return $"%{EscapeLikePattern(value.Trim())}%";
    }

    private static string EscapeLikePattern(string value)
    {
        return value
            .Replace(@"\", @"\\", StringComparison.Ordinal)
            .Replace("%", @"\%", StringComparison.Ordinal)
            .Replace("_", @"\_", StringComparison.Ordinal)
            .Replace("[", @"\[", StringComparison.Ordinal);
    }
}
