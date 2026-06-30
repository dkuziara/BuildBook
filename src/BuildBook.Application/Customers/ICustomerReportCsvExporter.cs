namespace BuildBook.Application.Customers;

public interface ICustomerReportCsvExporter
{
    Task<string> ExportAsync(
        CustomerReportFilter? filter = null,
        CancellationToken cancellationToken = default);
}
