namespace BuildBook.Application.Orders;

public interface IOrderDetailReader
{
    Task<OrderDetailModel?> GetByIdAsync(
        int orderRecordId,
        CancellationToken cancellationToken = default);
}
