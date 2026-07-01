namespace BuildBook.Application.Orders;

public interface IOrderReportCsvExporter
{
    Task<string> ExportAsync(
        OrderReportFilter? filter = null,
        CancellationToken cancellationToken = default);
}
