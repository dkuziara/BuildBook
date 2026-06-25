using BuildBook.Domain.BuildRecords;

namespace BuildBook.Application.BuildRecords;

public interface IBuildRecordSecretStore
{
    Task<string?> GetAsync(
        int buildRecordId,
        SecretType secretType,
        CancellationToken cancellationToken = default);

    Task SaveAsync(
        int buildRecordId,
        SecretType secretType,
        string secretValue,
        string updatedBy,
        CancellationToken cancellationToken = default);
}
