namespace BuildBook.Application.Customers;

public interface ICustomerService
{
    Task<IReadOnlyList<CustomerListItem>> SearchAsync(
        CustomerListFilter filter,
        CancellationToken cancellationToken = default);

    Task<CustomerDetailModel?> GetDetailAsync(
        int customerId,
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
}
