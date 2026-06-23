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
        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Development seed data added {BuildRecordCount} build records for {CustomerCount} customers.",
            seedData.BuildRecords.Count,
            seedData.Customers.Count);
    }
}
