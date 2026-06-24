using BuildBook.Application.BuildRecords;
using Microsoft.EntityFrameworkCore;

namespace BuildBook.Infrastructure.Persistence.BuildRecords;

public sealed class BuildRegisterReader(
    IDbContextFactory<BuildBookDbContext> dbContextFactory) : IBuildRegisterReader
{
    public async Task<IReadOnlyList<BuildRegisterRow>> ListAsync(
        CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        return await dbContext.BuildRecords
            .AsNoTracking()
            .Where(buildRecord => buildRecord.IsActive)
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
}
