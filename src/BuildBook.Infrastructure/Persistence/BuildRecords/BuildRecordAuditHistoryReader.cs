using BuildBook.Application.BuildRecords;
using Microsoft.EntityFrameworkCore;

namespace BuildBook.Infrastructure.Persistence.BuildRecords;

public sealed class BuildRecordAuditHistoryReader(
    IDbContextFactory<BuildBookDbContext> dbContextFactory) : IBuildRecordAuditHistoryReader
{
    public async Task<IReadOnlyList<BuildRecordAuditHistoryEntry>> ListByBuildRecordIdAsync(
        int buildRecordId,
        CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        return await dbContext.BuildRecordAudit
            .AsNoTracking()
            .Where(entry => entry.BuildRecordId == buildRecordId)
            .OrderByDescending(entry => entry.OccurredAt)
            .ThenByDescending(entry => entry.Id)
            .Select(entry => new BuildRecordAuditHistoryEntry(
                entry.Id,
                entry.OccurredAt,
                entry.User,
                entry.Action,
                entry.FieldChanged,
                entry.OldValue,
                entry.NewValue))
            .ToListAsync(cancellationToken);
    }
}
