using BuildBook.Application.BuildRecords;
using Microsoft.EntityFrameworkCore;

namespace BuildBook.Infrastructure.Persistence.BuildRecords;

public sealed class BuildRecordDetailReader(
    IDbContextFactory<BuildBookDbContext> dbContextFactory) : IBuildRecordDetailReader
{
    public async Task<BuildRecordDetailModel?> GetByIdAsync(
        int buildRecordId,
        CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        var buildRecord = await dbContext.BuildRecords
            .AsNoTracking()
            .Where(buildRecord => buildRecord.Id == buildRecordId && buildRecord.IsActive)
            .Select(buildRecord => new
            {
                buildRecord.Id,
                buildRecord.ProductCode,
                buildRecord.ProductName,
                buildRecord.ProductClassification,
                buildRecord.SerialNumber,
                buildRecord.InternalStatus,
                CustomerName = buildRecord.Customer == null ? null : buildRecord.Customer.Name,
                buildRecord.MachineName,
                buildRecord.RadSightVersion,
                buildRecord.WindowsVersion,
                buildRecord.DateShipped,
                buildRecord.LastUpdatedAt,
                buildRecord.LastUpdatedBy
            })
            .SingleOrDefaultAsync(cancellationToken);

        return buildRecord is null
            ? null
            : new BuildRecordDetailModel(
                buildRecord.Id,
                buildRecord.ProductCode,
                buildRecord.ProductName,
                buildRecord.ProductClassification,
                buildRecord.SerialNumber,
                buildRecord.InternalStatus?.ToString(),
                buildRecord.CustomerName,
                buildRecord.MachineName,
                buildRecord.RadSightVersion,
                buildRecord.WindowsVersion,
                buildRecord.DateShipped,
                buildRecord.LastUpdatedAt,
                buildRecord.LastUpdatedBy);
    }
}
