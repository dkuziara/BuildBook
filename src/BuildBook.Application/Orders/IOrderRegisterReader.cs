namespace BuildBook.Application.Orders;

public interface IOrderRegisterReader
{
    Task<IReadOnlyList<OrderRegisterRow>> ListAsync(
        OrderRegisterFilter? filter = null,
        CancellationToken cancellationToken = default);
}
