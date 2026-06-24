namespace BuildBook.Application.BuildRecords;

public sealed class UpdateCustomerShippingResult
{
    private UpdateCustomerShippingResult(bool succeeded, IReadOnlyList<string> errors)
    {
        Succeeded = succeeded;
        Errors = errors;
    }

    public bool Succeeded { get; }

    public IReadOnlyList<string> Errors { get; }

    public static UpdateCustomerShippingResult Success()
    {
        return new UpdateCustomerShippingResult(true, []);
    }

    public static UpdateCustomerShippingResult Failure(params string[] errors)
    {
        return new UpdateCustomerShippingResult(false, errors);
    }
}
