using BuildBook.Domain.BuildRecords;
using BuildBook.Infrastructure.Persistence;
using BuildBook.Infrastructure.Persistence.BuildRecords;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;

namespace BuildBook.Tests;

public class BuildRecordSecretServiceTests
{
    [Fact]
    public async Task SaveAsync_StoresSecretEncryptedAndAuditsChangeWithoutSecretValue()
    {
        var harness = await SecretServiceHarness.CreateAsync();

        try
        {
            var result = await harness.Service.SaveAsync(
                harness.BuildRecordId,
                SecretType.RouterPassword,
                "router-secret",
                "DOMAIN\\editor");

            Assert.True(result.Succeeded);

            await using var verifyContext = harness.CreateContext();
            var secretRow = await verifyContext.BuildRecordSecrets.SingleAsync();
            var encryptedText = System.Text.Encoding.UTF8.GetString(secretRow.SecretValueEncrypted);
            var auditEntry = await verifyContext.BuildRecordAudit.SingleAsync();
            var buildRecord = await verifyContext.BuildRecords.SingleAsync(record => record.Id == harness.BuildRecordId);

            Assert.NotEqual("router-secret", encryptedText);
            Assert.Equal("DOMAIN\\editor", secretRow.CreatedBy);
            Assert.Equal("DOMAIN\\editor", secretRow.LastUpdatedBy);
            Assert.Equal("DOMAIN\\editor", buildRecord.LastUpdatedBy);
            Assert.Equal(AuditAction.SensitiveValueChanged, auditEntry.Action);
            Assert.Equal(nameof(SecretType.RouterPassword), auditEntry.FieldChanged);
            Assert.Null(auditEntry.OldValue);
            Assert.Null(auditEntry.NewValue);
        }
        finally
        {
            await harness.DisposeAsync();
        }
    }

    [Fact]
    public async Task UpdateAsync_UpdatesExistingSecretWithoutAddingDuplicate()
    {
        var harness = await SecretServiceHarness.CreateAsync();

        try
        {
            await harness.Service.SaveAsync(
                harness.BuildRecordId,
                SecretType.WifiPassword,
                "first-secret",
                "editor");

            var result = await harness.Service.UpdateAsync(
                harness.BuildRecordId,
                SecretType.WifiPassword,
                "second-secret",
                "editor-2");

            Assert.True(result.Succeeded);

            await using var verifyContext = harness.CreateContext();
            var secretRow = await verifyContext.BuildRecordSecrets.SingleAsync();

            Assert.Equal(1, await verifyContext.BuildRecordSecrets.CountAsync());
            Assert.Equal("editor", secretRow.CreatedBy);
            Assert.Equal("editor-2", secretRow.LastUpdatedBy);
            Assert.Equal(2, await verifyContext.BuildRecordAudit.CountAsync(
                entry => entry.Action == AuditAction.SensitiveValueChanged));
        }
        finally
        {
            await harness.DisposeAsync();
        }
    }

    [Fact]
    public async Task RetrieveAsync_ReturnsSecretAndAuditsRevealWithoutSecretValue()
    {
        var harness = await SecretServiceHarness.CreateAsync();

        try
        {
            await harness.Service.SaveAsync(
                harness.BuildRecordId,
                SecretType.BitLockerRecoveryKey,
                "123456-123456-123456-123456-123456-123456-123456-123456",
                "editor");

            var result = await harness.Service.RetrieveAsync(
                harness.BuildRecordId,
                SecretType.BitLockerRecoveryKey,
                "viewer");

            Assert.True(result.Succeeded);
            Assert.Equal("123456-123456-123456-123456-123456-123456-123456-123456", result.SecretValue);

            await using var verifyContext = harness.CreateContext();
            var revealAuditEntry = await verifyContext.BuildRecordAudit
                .SingleAsync(entry => entry.Action == AuditAction.SensitiveValueViewed);

            Assert.Equal("viewer", revealAuditEntry.User);
            Assert.Equal(nameof(SecretType.BitLockerRecoveryKey), revealAuditEntry.FieldChanged);
            Assert.Null(revealAuditEntry.OldValue);
            Assert.Null(revealAuditEntry.NewValue);
        }
        finally
        {
            await harness.DisposeAsync();
        }
    }

    [Fact]
    public async Task SaveAsync_PreservesSecretValueExactly()
    {
        var harness = await SecretServiceHarness.CreateAsync();

        try
        {
            await harness.Service.SaveAsync(
                harness.BuildRecordId,
                SecretType.RadSightUserPassword,
                "  padded-secret  ",
                "editor");

            var result = await harness.Service.RetrieveAsync(
                harness.BuildRecordId,
                SecretType.RadSightUserPassword,
                "viewer");

            Assert.True(result.Succeeded);
            Assert.Equal("  padded-secret  ", result.SecretValue);
        }
        finally
        {
            await harness.DisposeAsync();
        }
    }

    [Fact]
    public async Task UpdateAsync_ReturnsFailureWhenSecretDoesNotExist()
    {
        var harness = await SecretServiceHarness.CreateAsync();

        try
        {
            var result = await harness.Service.UpdateAsync(
                harness.BuildRecordId,
                SecretType.KioskPassword,
                "new-secret",
                "editor");

            Assert.False(result.Succeeded);
            Assert.Contains("Secret value was not found.", result.Errors);

            await using var verifyContext = harness.CreateContext();
            Assert.Empty(await verifyContext.BuildRecordSecrets.ToListAsync());
            Assert.Empty(await verifyContext.BuildRecordAudit.ToListAsync());
        }
        finally
        {
            await harness.DisposeAsync();
        }
    }

    [Fact]
    public async Task SaveAsync_ReturnsFailureForBlankSecret()
    {
        var harness = await SecretServiceHarness.CreateAsync();

        try
        {
            var result = await harness.Service.SaveAsync(
                harness.BuildRecordId,
                SecretType.WindowsAdminPassword,
                "   ",
                "editor");

            Assert.False(result.Succeeded);
            Assert.Contains("Secret value must not be blank.", result.Errors);
        }
        finally
        {
            await harness.DisposeAsync();
        }
    }

    private sealed class SecretServiceHarness
    {
        private SecretServiceHarness(
            DbContextOptions<BuildBookDbContext> options,
            string keyDirectory,
            int buildRecordId,
            BuildRecordSecretService service)
        {
            Options = options;
            KeyDirectory = keyDirectory;
            BuildRecordId = buildRecordId;
            Service = service;
        }

        public DbContextOptions<BuildBookDbContext> Options { get; }

        public string KeyDirectory { get; }

        public int BuildRecordId { get; }

        public BuildRecordSecretService Service { get; }

        public static async Task<SecretServiceHarness> CreateAsync()
        {
            var databaseName = $"BuildBookSecretService_{Guid.NewGuid():N}";
            var connectionString = $"Server=(localdb)\\MSSQLLocalDB;Database={databaseName};Trusted_Connection=True;TrustServerCertificate=True";
            var options = new DbContextOptionsBuilder<BuildBookDbContext>()
                .UseSqlServer(connectionString)
                .Options;

            await using var setupContext = new BuildBookDbContext(options);
            await setupContext.Database.MigrateAsync();

            var buildRecord = new BuildRecord
            {
                ProductCode = "SECRET-SERVICE",
                ProductName = "Secret Service Test Device",
                SerialNumber = $"SECRET-SERVICE-{Guid.NewGuid():N}",
                CreatedBy = "tester",
                LastUpdatedBy = "tester"
            };

            setupContext.BuildRecords.Add(buildRecord);
            await setupContext.SaveChangesAsync();

            var keyDirectory = Path.Combine(Path.GetTempPath(), $"buildbook-secret-service-{Guid.NewGuid():N}");
            Directory.CreateDirectory(keyDirectory);
            var provider = DataProtectionProvider.Create(new DirectoryInfo(keyDirectory));
            var factory = new TestDbContextFactory(options);
            var service = new BuildRecordSecretService(factory, provider, new BuildRecordAuditService());

            return new SecretServiceHarness(options, keyDirectory, buildRecord.Id, service);
        }

        public BuildBookDbContext CreateContext()
        {
            return new BuildBookDbContext(Options);
        }

        public async Task DisposeAsync()
        {
            await using var context = CreateContext();
            await context.Database.EnsureDeletedAsync();
            Directory.Delete(KeyDirectory, recursive: true);
        }
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
