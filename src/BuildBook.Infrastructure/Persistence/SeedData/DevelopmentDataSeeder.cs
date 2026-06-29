using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BuildBook.Infrastructure.Persistence.SeedData;

public sealed class DevelopmentDataSeeder(
    IDbContextFactory<BuildBookDbContext> dbContextFactory,
    ILogger<DevelopmentDataSeeder> logger)
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        if (await context.BuildRecords.AnyAsync(cancellationToken))
        {
            logger.LogInformation("Development seed data skipped because build records already exist.");
            return;
        }

        var seedData = DevelopmentSeedData.Create();

        await context.Customers.AddRangeAsync(seedData.Customers, cancellationToken);
        await context.Imports.AddAsync(seedData.ImportBatch, cancellationToken);
        await context.BuildRecords.AddRangeAsync(seedData.BuildRecords, cancellationToken);
        await context.BuildRecordAudit.AddRangeAsync(seedData.AuditEntries, cancellationToken);
        await context.RmaRecords.AddRangeAsync(seedData.RmaRecords, cancellationToken);
        await context.RmaChecklistItems.AddRangeAsync(seedData.RmaChecklistItems, cancellationToken);
        await context.RmaNotes.AddRangeAsync(seedData.RmaNotes, cancellationToken);
        await context.RmaCommunications.AddRangeAsync(seedData.RmaCommunications, cancellationToken);
        await context.RmaAttachments.AddRangeAsync(seedData.RmaAttachments, cancellationToken);
        await context.RmaParts.AddRangeAsync(seedData.RmaParts, cancellationToken);
        await context.RmaStatusHistory.AddRangeAsync(seedData.RmaStatusHistoryEntries, cancellationToken);
        await context.RmaAudit.AddRangeAsync(seedData.RmaAuditEntries, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Development seed data added {BuildRecordCount} build records and {RmaRecordCount} RMA records for {CustomerCount} customers.",
            seedData.BuildRecords.Count,
            seedData.RmaRecords.Count,
            seedData.Customers.Count);
    }
}
