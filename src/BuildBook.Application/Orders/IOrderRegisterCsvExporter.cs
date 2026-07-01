namespace BuildBook.Application.Orders;

public interface IOrderRegisterCsvExporter
{
    Task<string> ExportAsync(
        OrderRegisterFilter? filter = null,
        CancellationToken cancellationToken = default);
}
