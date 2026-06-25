using BuildBook.Application.BuildRecords;
using BuildBook.Domain.BuildRecords;
using BuildBook.Infrastructure.Persistence;
using BuildBook.Infrastructure.Persistence.BuildRecords;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;

namespace BuildBook.Tests;

public class BuildRecordSecretStoreTests
{
    [Fact]
    public async Task SaveAsync_StoresEncryptedValueSeparatelyAndGetAsyncReturnsOriginal()
    {
        var databaseName = $"BuildBookSecretStore_{Guid.NewGuid():N}";
        var connectionString = $"Server=(localdb)\\MSSQLLocalDB;Database={databaseName};Trusted_Connection=True;TrustServerCertificate=True";
        var options = new DbContextOptionsBuilder<BuildBookDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        await using var setupContext = new BuildBookDbContext(options);
        await setupContext.Database.MigrateAsync();

        var buildRecord = new BuildRecord
        {
            ProductCode = "SECRET-1",
            ProductName = "Secret Test Device",
            SerialNumber = "SECRET-DEVICE-1",
            CreatedBy = "tester",
            LastUpdatedBy = "tester"
        };

        setupContext.BuildRecords.Add(buildRecord);
        await setupContext.SaveChangesAsync();

        var keyDirectory = Path.Combine(Path.GetTempPath(), $"buildbook-secrets-{Guid.NewGuid():N}");
        Directory.CreateDirectory(keyDirectory);
        var provider = DataProtectionProvider.Create(new DirectoryInfo(keyDirectory));
        var factory = new TestDbContextFactory(options);
        var store = new BuildRecordSecretStore(factory, provider);

        await store.SaveAsync(buildRecord.Id, SecretType.RouterPassword, "super-secret", "tester");

        await using var verifyContext = new BuildBookDbContext(options);
        var secretRow = await verifyContext.BuildRecordSecrets.SingleAsync();
        var encryptedText = System.Text.Encoding.UTF8.GetString(secretRow.SecretValueEncrypted);

        Assert.NotEqual("super-secret", encryptedText);
        Assert.Equal("tester", secretRow.CreatedBy);
        Assert.Equal("tester", secretRow.LastUpdatedBy);

        var revealedValue = await store.GetAsync(buildRecord.Id, SecretType.RouterPassword);
        Assert.Equal("super-secret", revealedValue);

        await verifyContext.Database.EnsureDeletedAsync();
        Directory.Delete(keyDirectory, recursive: true);
    }

    [Fact]
    public async Task SaveAsync_UpdatesExistingSecretInsteadOfAddingDuplicate()
    {
        var databaseName = $"BuildBookSecretStore_{Guid.NewGuid():N}";
        var connectionString = $"Server=(localdb)\\MSSQLLocalDB;Database={databaseName};Trusted_Connection=True;TrustServerCertificate=True";
        var options = new DbContextOptionsBuilder<BuildBookDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        await using var setupContext = new BuildBookDbContext(options);
        await setupContext.Database.MigrateAsync();

        var buildRecord = new BuildRecord
        {
            ProductCode = "SECRET-2",
            ProductName = "Secret Test Device 2",
            SerialNumber = "SECRET-DEVICE-2",
            CreatedBy = "tester",
            LastUpdatedBy = "tester"
        };

        setupContext.BuildRecords.Add(buildRecord);
        await setupContext.SaveChangesAsync();

        var keyDirectory = Path.Combine(Path.GetTempPath(), $"buildbook-secrets-{Guid.NewGuid():N}");
        Directory.CreateDirectory(keyDirectory);
        var provider = DataProtectionProvider.Create(new DirectoryInfo(keyDirectory));
        var factory = new TestDbContextFactory(options);
        var store = new BuildRecordSecretStore(factory, provider);

        await store.SaveAsync(buildRecord.Id, SecretType.WifiPassword, "first-secret", "tester");
        await store.SaveAsync(buildRecord.Id, SecretType.WifiPassword, "second-secret", "tester-2");

        await using var verifyContext = new BuildBookDbContext(options);
        Assert.Equal(1, await verifyContext.BuildRecordSecrets.CountAsync());

        var secretRow = await verifyContext.BuildRecordSecrets.SingleAsync();
        Assert.Equal("tester", secretRow.CreatedBy);
        Assert.Equal("tester-2", secretRow.LastUpdatedBy);

        var revealedValue = await store.GetAsync(buildRecord.Id, SecretType.WifiPassword);
        Assert.Equal("second-secret", revealedValue);

        await verifyContext.Database.EnsureDeletedAsync();
        Directory.Delete(keyDirectory, recursive: true);
    }

    private sealed class TestDbContextFactory(DbContextOptions<BuildBookDbContext> options) : IDbContextFactory<BuildBookDbContext>
    {
        public BuildBookDbContext CreateDbContext()
        {
            return new BuildBookDbContext(options);
        }

        public Task<BuildBookDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new BuildBookDbContext(options));
        }
    }
}
