namespace BuildBook.Application.Customers;

public interface ICustomerListExcelExporter
{
    Task<byte[]> ExportAsync(
        CustomerListFilter filter,
        CancellationToken cancellationToken = default);
}
