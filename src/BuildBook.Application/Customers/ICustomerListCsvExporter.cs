namespace BuildBook.Application.Customers;

public interface ICustomerListCsvExporter
{
    Task<string> ExportAsync(
        CustomerListFilter filter,
        CancellationToken cancellationToken = default);
}
