using BuildBook.Application.Security;
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
}
