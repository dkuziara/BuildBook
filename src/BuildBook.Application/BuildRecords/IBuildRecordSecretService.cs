using BuildBook.Domain.BuildRecords;

namespace BuildBook.Application.BuildRecords;

public interface IBuildRecordSecretService
{
    Task<BuildRecordSecretSaveResult> SaveAsync(
        int buildRecordId,
        SecretType secretType,
        string secretValue,
        string updatedBy,
        CancellationToken cancellationToken = default);

    Task<BuildRecordSecretSaveResult> UpdateAsync(
        int buildRecordId,
        SecretType secretType,
        string secretValue,
        string updatedBy,
        CancellationToken cancellationToken = default);

    Task<BuildRecordSecretRevealResult> RetrieveAsync(
        int buildRecordId,
        SecretType secretType,
        string viewedBy,
        CancellationToken cancellationToken = default);
}
