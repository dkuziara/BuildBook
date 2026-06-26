using BuildBook.Application.BuildRecords;
using BuildBook.Domain.BuildRecords;
using Microsoft.EntityFrameworkCore;

namespace BuildBook.Infrastructure.Persistence.BuildRecords;

public sealed class MissingDataReportReader(
    IDbContextFactory<BuildBookDbContext> dbContextFactory) : IMissingDataReportReader
{
    public async Task<IReadOnlyList<MissingDataReportRow>> ListActiveAsync(
        CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        var buildRecords = await dbContext.BuildRecords
            .AsNoTracking()
            .Where(buildRecord => buildRecord.IsActive)
            .Select(buildRecord => new
            {
                buildRecord.Id,
                buildRecord.ProductCode,
                buildRecord.ProductName,
                buildRecord.SerialNumber,
                CustomerName = buildRecord.Customer == null ? null : buildRecord.Customer.Name,
                buildRecord.MachineName,
                buildRecord.RadSightVersion,
                buildRecord.WindowsVersion,
                buildRecord.DateShipped,
                buildRecord.LastUpdatedAt,
                IsMissingCustomer = buildRecord.CustomerId == null,
                IsMissingDateShipped = buildRecord.DateShipped == null
            })
            .OrderByDescending(buildRecord => buildRecord.LastUpdatedAt)
            .ThenBy(buildRecord => buildRecord.SerialNumber)
            .ToListAsync(cancellationToken);

        var buildRecordIds = buildRecords.Select(buildRecord => buildRecord.Id).ToList();
        var buildRecordIdsWithRecoveryKeys = await dbContext.BuildRecordSecrets
            .AsNoTracking()
            .Where(secret =>
                buildRecordIds.Contains(secret.BuildRecordId)
                && secret.SecretType == SecretType.BitLockerRecoveryKey)
            .Select(secret => secret.BuildRecordId)
            .Distinct()
            .ToListAsync(cancellationToken);
        var recoveryKeyBuildRecordIds = buildRecordIdsWithRecoveryKeys.ToHashSet();

        return buildRecords
            .Select(buildRecord => new MissingDataReportRow(
                buildRecord.Id,
                buildRecord.ProductCode,
                buildRecord.ProductName,
                buildRecord.SerialNumber,
                buildRecord.CustomerName,
                buildRecord.MachineName,
                buildRecord.RadSightVersion,
                buildRecord.WindowsVersion,
                buildRecord.DateShipped,
                buildRecord.LastUpdatedAt,
                buildRecord.IsMissingCustomer,
                !recoveryKeyBuildRecordIds.Contains(buildRecord.Id),
                buildRecord.IsMissingDateShipped))
            .ToList();
    }
}
