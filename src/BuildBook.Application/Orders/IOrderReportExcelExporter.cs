namespace BuildBook.Application.Orders;

public interface IOrderReportExcelExporter
{
    Task<byte[]> ExportAsync(
        OrderReportFilter? filter = null,
        CancellationToken cancellationToken = default);
}
