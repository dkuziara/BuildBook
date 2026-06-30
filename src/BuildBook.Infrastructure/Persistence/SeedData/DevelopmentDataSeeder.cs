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
        var supportContractLevelsByName = await context.SupportContractLevels
            .ToDictionaryAsync(level => level.Name, StringComparer.Ordinal, cancellationToken);

        foreach (var customer in seedData.Customers)
        {
            if (customer.SupportContractLevel is null)
            {
                continue;
            }

            if (supportContractLevelsByName.TryGetValue(customer.SupportContractLevel.Name, out var existingLevel))
            {
                customer.SupportContractLevel = existingLevel;
                customer.SupportContractLevelId = existingLevel.Id;
            }
        }

        if (supportContractLevelsByName.Count == 0)
        {
            await context.SupportContractLevels.AddRangeAsync(seedData.SupportContractLevels, cancellationToken);
        }

        await context.Customers.AddRangeAsync(seedData.Customers, cancellationToken);

        var existingSettingKeys = await context.SystemSettings
            .Select(setting => setting.Key)
            .ToListAsync(cancellationToken);
        var missingSettings = seedData.SystemSettings
            .Where(setting => !existingSettingKeys.Contains(setting.Key, StringComparer.Ordinal))
            .ToArray();

        if (missingSettings.Length > 0)
        {
            await context.SystemSettings.AddRangeAsync(missingSettings, cancellationToken);
        }

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
            "Development seed data added {BuildRecordCount} build records, {RmaRecordCount} RMA records, {CustomerCount} customers and {SupportContractLevelCount} support contract levels.",
            seedData.BuildRecords.Count,
            seedData.RmaRecords.Count,
            seedData.Customers.Count,
            seedData.SupportContractLevels.Count);
    }
}
