namespace BuildBook.Application.Customers;

public interface ICustomerListReader
{
    Task<IReadOnlyList<CustomerListItem>> ListAsync(
        CustomerListFilter filter,
        CancellationToken cancellationToken = default);
}
