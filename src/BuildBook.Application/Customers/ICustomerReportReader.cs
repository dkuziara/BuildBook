namespace BuildBook.Application.Customers;

public interface ICustomerReportReader
{
    Task<IReadOnlyList<CustomerContractReportRow>> ListCustomersAsync(
        CustomerReportFilter? filter = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CustomerSupportRmaReportRow>> ListRmasAsync(
        CustomerReportFilter? filter = null,
        CancellationToken cancellationToken = default);
}
