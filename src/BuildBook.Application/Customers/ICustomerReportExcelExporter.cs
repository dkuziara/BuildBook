namespace BuildBook.Application.Customers;

public interface ICustomerReportExcelExporter
{
    Task<byte[]> ExportAsync(
        CustomerReportFilter? filter = null,
        CancellationToken cancellationToken = default);
}
