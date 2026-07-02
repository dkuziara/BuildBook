using BuildBook.Application.Paging;

namespace BuildBook.Application.Customers;

public interface ICustomerService
{
    Task<PagedResult<CustomerListItem>> SearchPageAsync(
        CustomerListFilter filter,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CustomerListItem>> SearchAsync(
        CustomerListFilter filter,
        CancellationToken cancellationToken = default);

    Task<CustomerDetailModel?> GetDetailAsync(
        int customerId,
        CancellationToken cancellationToken = default);

    Task<CustomerContractDocumentContentModel?> GetContractDocumentContentAsync(
        int customerId,
        int documentId,
        CancellationToken cancellationToken = default);

    Task<CustomerSaveResult> CreateAsync(
        CreateCustomerRequest request,
        string createdBy,
        CancellationToken cancellationToken = default);

    Task<CustomerSaveResult> UpdateAsync(
        int customerId,
        UpdateCustomerRequest request,
        string updatedBy,
        CancellationToken cancellationToken = default);

    Task<CustomerSaveResult> SaveContractDocumentAsync(
        int customerId,
        SaveCustomerContractDocumentRequest request,
        Stream content,
        string uploadedBy,
        CancellationToken cancellationToken = default);

    Task<CustomerSaveResult> DeleteContractDocumentAsync(
        int customerId,
        int documentId,
        string deletedBy,
        CancellationToken cancellationToken = default);
}
