using System.Text;
using BuildBook.Application.BuildRecords;
using BuildBook.Domain.BuildRecords;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;

namespace BuildBook.Infrastructure.Persistence.BuildRecords;

public sealed class BuildRecordSecretService(
    IDbContextFactory<BuildBookDbContext> dbContextFactory,
    IDataProtectionProvider dataProtectionProvider,
    IBuildRecordAuditService buildRecordAuditService) : IBuildRecordSecretService
{
    private const string SecretProtectionPurpose = "BuildBook.BuildRecordSecrets.v1";
    private readonly IDataProtector protector = dataProtectionProvider.CreateProtector(SecretProtectionPurpose);

    public Task<BuildRecordSecretSaveResult> SaveAsync(
        int buildRecordId,
        SecretType secretType,
        string secretValue,
        string updatedBy,
        CancellationToken cancellationToken = default)
    {
        return SaveOrUpdateAsync(
            buildRecordId,
            secretType,
            secretValue,
            updatedBy,
            requiresExistingSecret: false,
            cancellationToken);
    }

    public Task<BuildRecordSecretSaveResult> UpdateAsync(
        int buildRecordId,
        SecretType secretType,
        string secretValue,
        string updatedBy,
        CancellationToken cancellationToken = default)
    {
        return SaveOrUpdateAsync(
            buildRecordId,
            secretType,
            secretValue,
            updatedBy,
            requiresExistingSecret: true,
            cancellationToken);
    }

    public async Task<BuildRecordSecretRevealResult> RetrieveAsync(
        int buildRecordId,
        SecretType secretType,
        string viewedBy,
        CancellationToken cancellationToken = default)
    {
        var userName = NormalizeUserName(viewedBy);

        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        var buildRecord = await dbContext.BuildRecords
            .SingleOrDefaultAsync(
                record => record.Id == buildRecordId && record.IsActive,
                cancellationToken);

        if (buildRecord is null)
        {
            return BuildRecordSecretRevealResult.Failure("Build Record was not found.");
        }

        var secret = await dbContext.BuildRecordSecrets
            .AsNoTracking()
            .SingleOrDefaultAsync(
                secret => secret.BuildRecordId == buildRecordId && secret.SecretType == secretType,
                cancellationToken);

        if (secret is null)
        {
            return BuildRecordSecretRevealResult.Success(null);
        }

        var secretValue = Unprotect(secret.SecretValueEncrypted);
        var auditEntry = buildRecordAuditService.CreateSensitiveValueViewedEntry(
            buildRecord,
            secretType.ToString(),
            userName);

        await dbContext.BuildRecordAudit.AddAsync(auditEntry, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return BuildRecordSecretRevealResult.Success(secretValue);
    }

    private async Task<BuildRecordSecretSaveResult> SaveOrUpdateAsync(
        int buildRecordId,
        SecretType secretType,
        string secretValue,
        string updatedBy,
        bool requiresExistingSecret,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(secretValue))
        {
            return BuildRecordSecretSaveResult.Failure("Secret value must not be blank.");
        }

        var userName = NormalizeUserName(updatedBy);

        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        var buildRecord = await dbContext.BuildRecords
            .SingleOrDefaultAsync(
                record => record.Id == buildRecordId && record.IsActive,
                cancellationToken);

        if (buildRecord is null)
        {
            return BuildRecordSecretSaveResult.Failure("Build Record was not found.");
        }

        var existingSecret = await dbContext.BuildRecordSecrets
            .SingleOrDefaultAsync(
                secret => secret.BuildRecordId == buildRecordId && secret.SecretType == secretType,
                cancellationToken);

        if (existingSecret is null && requiresExistingSecret)
        {
            return BuildRecordSecretSaveResult.Failure("Secret value was not found.");
        }

        if (existingSecret is not null && string.Equals(
            Unprotect(existingSecret.SecretValueEncrypted),
            secretValue,
            StringComparison.Ordinal))
        {
            return BuildRecordSecretSaveResult.Success();
        }

        var now = DateTimeOffset.UtcNow;
        var protectedValue = Protect(secretValue);

        if (existingSecret is null)
        {
            dbContext.BuildRecordSecrets.Add(new BuildRecordSecret
            {
                BuildRecordId = buildRecordId,
                SecretType = secretType,
                SecretValueEncrypted = protectedValue,
                CreatedAt = now,
                CreatedBy = userName,
                LastUpdatedAt = now,
                LastUpdatedBy = userName
            });
        }
        else
        {
            existingSecret.SecretValueEncrypted = protectedValue;
            existingSecret.LastUpdatedAt = now;
            existingSecret.LastUpdatedBy = userName;
        }

        buildRecord.LastUpdatedAt = now;
        buildRecord.LastUpdatedBy = userName;

        var auditEntry = buildRecordAuditService.CreateSensitiveValueChangedEntry(
            buildRecord,
            secretType.ToString(),
            userName);

        await dbContext.BuildRecordAudit.AddAsync(auditEntry, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return BuildRecordSecretSaveResult.Success();
    }

    private byte[] Protect(string secretValue)
    {
        return Encoding.UTF8.GetBytes(protector.Protect(secretValue));
    }

    private string Unprotect(byte[] protectedValue)
    {
        return protector.Unprotect(Encoding.UTF8.GetString(protectedValue));
    }

    private static string NormalizeUserName(string userName)
    {
        return string.IsNullOrWhiteSpace(userName) ? "Unknown" : userName.Trim();
    }
}
