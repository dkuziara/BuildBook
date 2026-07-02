using BuildBook.Application.Paging;

namespace BuildBook.Application.Orders;

public interface IOrderRegisterReader
{
    Task<PagedResult<OrderRegisterRow>> GetPageAsync(
        OrderRegisterFilter? filter,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<OrderRegisterRow>> ListAsync(
        OrderRegisterFilter? filter = null,
        CancellationToken cancellationToken = default);
}
