namespace BuildBook.Application.Customers;

public interface ISupportContractLevelService
{
    Task<IReadOnlyList<SupportContractLevelModel>> ListAsync(
        bool includeInactive = true,
        CancellationToken cancellationToken = default);

    Task<SupportContractLevelSaveResult> CreateAsync(
        CreateSupportContractLevelRequest request,
        string createdBy,
        CancellationToken cancellationToken = default);

    Task<SupportContractLevelSaveResult> UpdateAsync(
        int supportContractLevelId,
        UpdateSupportContractLevelRequest request,
        string updatedBy,
        CancellationToken cancellationToken = default);
}
