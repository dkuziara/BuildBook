namespace BuildBook.Application.Customers;

public sealed class CustomerSaveResult
{
    private CustomerSaveResult(bool succeeded, int? customerId, IReadOnlyList<string> errors)
    {
        Succeeded = succeeded;
        CustomerId = customerId;
        Errors = errors;
    }

    public bool Succeeded { get; }

    public int? CustomerId { get; }

    public IReadOnlyList<string> Errors { get; }

    public static CustomerSaveResult Success(int customerId)
    {
        return new CustomerSaveResult(true, customerId, []);
    }

    public static CustomerSaveResult Failure(params string[] errors)
    {
        return new CustomerSaveResult(false, null, errors);
    }
}
