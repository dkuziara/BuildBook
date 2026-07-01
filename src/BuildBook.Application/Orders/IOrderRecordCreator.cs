namespace BuildBook.Application.Orders;

public interface IOrderRecordCreator
{
    Task<CreateOrderResult> CreateAsync(
        CreateOrderRequest request,
        string createdBy,
        CancellationToken cancellationToken = default);
}
