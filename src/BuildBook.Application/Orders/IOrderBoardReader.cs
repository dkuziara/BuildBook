namespace BuildBook.Application.Orders;

public interface IOrderBoardReader
{
    Task<IReadOnlyList<OrderBoardCardModel>> GetBoardAsync(
        CancellationToken cancellationToken = default);
}
