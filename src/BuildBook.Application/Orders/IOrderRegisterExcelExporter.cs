namespace BuildBook.Application.Orders;

public interface IOrderRegisterExcelExporter
{
    Task<byte[]> ExportAsync(
        OrderRegisterFilter? filter = null,
        CancellationToken cancellationToken = default);
}
