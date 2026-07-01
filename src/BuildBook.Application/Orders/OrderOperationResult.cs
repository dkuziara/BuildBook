namespace BuildBook.Application.Orders;

public sealed class OrderOperationResult
{
    private OrderOperationResult(bool succeeded, IReadOnlyList<string> errors)
    {
        Succeeded = succeeded;
        Errors = errors;
    }

    public bool Succeeded { get; }

    public IReadOnlyList<string> Errors { get; }

    public static OrderOperationResult Success() => new(true, []);

    public static OrderOperationResult Failure(params string[] errors) => new(false, errors);

    public static OrderOperationResult Failure(IReadOnlyList<string> errors) => new(false, errors);
}
