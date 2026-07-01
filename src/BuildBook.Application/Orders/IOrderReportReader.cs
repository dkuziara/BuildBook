namespace BuildBook.Application.Orders;

public interface IOrderReportReader
{
    Task<IReadOnlyList<OrderReportRow>> ListAsync(
        OrderReportFilter? filter = null,
        CancellationToken cancellationToken = default);
}
