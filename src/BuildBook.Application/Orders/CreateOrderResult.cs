namespace BuildBook.Application.Orders;

public sealed class CreateOrderResult
{
    private CreateOrderResult(bool succeeded, int? orderId, IReadOnlyList<string> errors)
    {
        Succeeded = succeeded;
        OrderId = orderId;
        Errors = errors;
    }

    public bool Succeeded { get; }

    public int? OrderId { get; }

    public IReadOnlyList<string> Errors { get; }

    public static CreateOrderResult Success(int orderId)
    {
        return new CreateOrderResult(true, orderId, []);
    }

    public static CreateOrderResult Failure(params string[] errors)
    {
        return new CreateOrderResult(false, null, errors);
    }

    public static CreateOrderResult Failure(IReadOnlyList<string> errors)
    {
        return new CreateOrderResult(false, null, errors);
    }
}
