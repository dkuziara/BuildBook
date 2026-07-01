using BuildBook.Application.Orders;
using BuildBook.Application.Security;
using BuildBook.Application.Settings;
using BuildBook.Domain.Customers;
using BuildBook.Domain.Rmas;
using BuildBook.Domain.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BuildBook.Infrastructure.Persistence;

public sealed class BuildBookDatabaseInitializer(
    IDbContextFactory<BuildBookDbContext> dbContextFactory,
    ILogger<BuildBookDatabaseInitializer> logger)
{
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var databaseExists = await dbContext.Database.CanConnectAsync(cancellationToken);
        var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync(cancellationToken);

        if (pendingMigrations.Any())
        {
            if (databaseExists)
            {
                logger.LogInformation(
                    "Applying {MigrationCount} pending database migrations for BuildBook.",
                    pendingMigrations.Count());
            }
            else
            {
                logger.LogInformation(
                    "BuildBook database does not exist yet. Creating database and applying {MigrationCount} migrations.",
                    pendingMigrations.Count());
            }

            await dbContext.Database.MigrateAsync(cancellationToken);

            logger.LogInformation(
                databaseExists
                    ? "BuildBook database migration completed."
                    : "BuildBook database was created and initialized.");
        }

        await EnsureApplicationRolesAsync(dbContext, cancellationToken);
        await EnsureSupportContractLevelsAsync(dbContext, cancellationToken);
        await EnsureSystemSettingsAsync(dbContext, cancellationToken);
    }

    private static async Task EnsureApplicationRolesAsync(
        BuildBookDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var existingRoleNames = await dbContext.ApplicationRoles
            .Select(role => role.Name)
            .ToListAsync(cancellationToken);

        var missingRoles = BuildBookRoles.All
            .Except(existingRoleNames, StringComparer.Ordinal)
            .Select(roleName => new Domain.Security.ApplicationRole
            {
                Name = roleName
            })
            .ToArray();

        if (missingRoles.Length == 0)
        {
            return;
        }

        await dbContext.ApplicationRoles.AddRangeAsync(missingRoles, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static async Task EnsureSupportContractLevelsAsync(
        BuildBookDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var configuredLevels = new[]
        {
            CreateSupportContractLevel(
                "Bronze",
                "Basic support",
                2,
                SupportResponseTimeUnit.WorkingDays,
                RmaPriority.Medium,
                10,
                1),
            CreateSupportContractLevel(
                "Silver",
                "Standard support",
                1,
                SupportResponseTimeUnit.WorkingDays,
                RmaPriority.Medium,
                20,
                2),
            CreateSupportContractLevel(
                "Gold",
                "Priority support",
                4,
                SupportResponseTimeUnit.WorkingHours,
                RmaPriority.High,
                30,
                3)
        };

        var existingNames = await dbContext.SupportContractLevels
            .Select(level => level.Name)
            .ToListAsync(cancellationToken);

        var missingLevels = configuredLevels
            .Where(level => !existingNames.Contains(level.Name, StringComparer.Ordinal))
            .ToArray();

        if (missingLevels.Length == 0)
        {
            return;
        }

        await dbContext.SupportContractLevels.AddRangeAsync(missingLevels, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static SupportContractLevel CreateSupportContractLevel(
        string name,
        string description,
        int responseTimeValue,
        SupportResponseTimeUnit responseTimeUnit,
        RmaPriority defaultRmaPriority,
        int priorityWeight,
        int displayOrder)
    {
        return new SupportContractLevel
        {
            Name = name,
            Description = description,
            TargetResponseTimeValue = responseTimeValue,
            TargetResponseTimeUnit = responseTimeUnit,
            DefaultRmaPriority = defaultRmaPriority,
            RmaPriorityWeight = priorityWeight,
            DisplayOrder = displayOrder,
            IsActive = true,
            CreatedBy = "BuildBook initialization",
            LastUpdatedBy = "BuildBook initialization"
        };
    }

    private static async Task EnsureSystemSettingsAsync(
        BuildBookDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var existingKeys = await dbContext.SystemSettings
            .Select(setting => setting.Key)
            .ToListAsync(cancellationToken);

        var missingSettings = new[]
        {
            new SystemSetting
            {
                Key = SystemSettingKeys.OrderWorkflowStatuses,
                Value = BuildBookOrderStatuses.SerializeDefaultWorkflow(),
                Description = "Default seeded workflow statuses used by the Orders module foundation.",
                LastUpdatedBy = "BuildBook initialization"
            },
            new SystemSetting
            {
                Key = SystemSettingKeys.SupportTicketUrlTemplate,
                Description = "Template used to open the support site for a support ticket number.",
                LastUpdatedBy = "BuildBook initialization"
            },
            new SystemSetting
            {
                Key = SystemSettingKeys.SupportTicketLabel,
                Value = "Support Ticket No.",
                Description = "Display label used for support ticket identifiers.",
                LastUpdatedBy = "BuildBook initialization"
            }
        }
        .Where(setting => !existingKeys.Contains(setting.Key, StringComparer.Ordinal))
        .ToArray();

        if (missingSettings.Length == 0)
        {
            return;
        }

        await dbContext.SystemSettings.AddRangeAsync(missingSettings, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
