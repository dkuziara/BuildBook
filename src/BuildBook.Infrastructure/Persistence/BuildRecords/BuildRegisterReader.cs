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

        return await query
            .OrderByDescending(buildRecord => buildRecord.LastUpdatedAt)
            .ThenBy(buildRecord => buildRecord.SerialNumber)
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
