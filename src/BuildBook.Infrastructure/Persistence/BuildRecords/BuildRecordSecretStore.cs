using System.Text;
using BuildBook.Application.BuildRecords;
using BuildBook.Domain.BuildRecords;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;

namespace BuildBook.Infrastructure.Persistence.BuildRecords;

public sealed class BuildRecordSecretStore(
    IDbContextFactory<BuildBookDbContext> dbContextFactory,
    IDataProtectionProvider dataProtectionProvider) : IBuildRecordSecretStore
{
    private const string SecretProtectionPurpose = "BuildBook.BuildRecordSecrets.v1";
    private readonly IDataProtector protector = dataProtectionProvider.CreateProtector(SecretProtectionPurpose);

    public async Task<string?> GetAsync(
        int buildRecordId,
        SecretType secretType,
        CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        var encryptedValue = await dbContext.BuildRecordSecrets
            .AsNoTracking()
            .Where(secret => secret.BuildRecordId == buildRecordId && secret.SecretType == secretType)
            .Select(secret => secret.SecretValueEncrypted)
            .SingleOrDefaultAsync(cancellationToken);

        if (encryptedValue is null)
        {
            return null;
        }

        var protectedValue = Encoding.UTF8.GetString(encryptedValue);
        return protector.Unprotect(protectedValue);
    }

    public async Task SaveAsync(
        int buildRecordId,
        SecretType secretType,
        string secretValue,
        string updatedBy,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(secretValue))
        {
            throw new ArgumentException("Secret value must not be blank.", nameof(secretValue));
        }

        var userName = string.IsNullOrWhiteSpace(updatedBy) ? "Unknown" : updatedBy.Trim();

        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        var buildRecordExists = await dbContext.BuildRecords
            .AnyAsync(record => record.Id == buildRecordId && record.IsActive, cancellationToken);

        if (!buildRecordExists)
        {
            throw new InvalidOperationException("Build Record was not found.");
        }

        var protectedValue = protector.Protect(secretValue);
        var encryptedBytes = Encoding.UTF8.GetBytes(protectedValue);
        var existingSecret = await dbContext.BuildRecordSecrets
            .SingleOrDefaultAsync(
                secret => secret.BuildRecordId == buildRecordId && secret.SecretType == secretType,
                cancellationToken);

        if (existingSecret is null)
        {
            dbContext.BuildRecordSecrets.Add(new BuildRecordSecret
            {
                BuildRecordId = buildRecordId,
                SecretType = secretType,
                SecretValueEncrypted = encryptedBytes,
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = userName,
                LastUpdatedAt = DateTimeOffset.UtcNow,
                LastUpdatedBy = userName
            });
        }
        else
        {
            existingSecret.SecretValueEncrypted = encryptedBytes;
            existingSecret.LastUpdatedAt = DateTimeOffset.UtcNow;
            existingSecret.LastUpdatedBy = userName;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
