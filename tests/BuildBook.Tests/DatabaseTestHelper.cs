using BuildBook.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BuildBook.Tests;

internal static class DatabaseTestHelper
{
    public static DbContextOptions<BuildBookDbContext> CreateSqlServerOptions(string databasePrefix)
    {
        var databaseName = $"{databasePrefix}_{Guid.NewGuid():N}";
        var connectionString = $"Server=(localdb)\\MSSQLLocalDB;Database={databaseName};Trusted_Connection=True;TrustServerCertificate=True";

        return new DbContextOptionsBuilder<BuildBookDbContext>()
            .UseSqlServer(connectionString)
            .Options;
    }

    public static async Task InitializeDatabaseAsync(DbContextOptions<BuildBookDbContext> options)
    {
        await using var context = new BuildBookDbContext(options);
        await context.Database.MigrateAsync();
    }

    public static async Task DeleteDatabaseAsync(DbContextOptions<BuildBookDbContext> options)
    {
        await using var context = new BuildBookDbContext(options);
        await context.Database.EnsureDeletedAsync();
    }
}

internal sealed class TestDbContextFactory(DbContextOptions<BuildBookDbContext> options) : IDbContextFactory<BuildBookDbContext>
{
    public BuildBookDbContext CreateDbContext() => new(options);

    public Task<BuildBookDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new BuildBookDbContext(options));
    }
}
