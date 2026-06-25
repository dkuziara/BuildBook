using BuildBook.Application.BuildRecords;
using Microsoft.EntityFrameworkCore;

namespace BuildBook.Infrastructure.Persistence.BuildRecords;

public sealed class ImportHistoryReader(
    IDbContextFactory<BuildBookDbContext> dbContextFactory) : IImportHistoryReader
{
    public async Task<IReadOnlyList<ImportHistoryEntry>> ListAsync(
        CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        return await dbContext.Imports
            .AsNoTracking()
            .OrderByDescending(importBatch => importBatch.ImportedAt)
            .ThenByDescending(importBatch => importBatch.Id)
            .Select(importBatch => new ImportHistoryEntry(
                importBatch.Id,
                importBatch.SourceFileName,
                importBatch.ImportedAt,
                importBatch.ImportedBy,
                importBatch.Status,
                importBatch.RowsRead,
                importBatch.RecordsCreated,
                importBatch.RecordsSkipped,
                importBatch.WarningCount,
                importBatch.ErrorCount,
                importBatch.Summary ?? string.Empty))
            .ToListAsync(cancellationToken);
    }
}
