namespace BuildBook.Application.Rmas;

public sealed class RmaOperationResult
{
    private RmaOperationResult(bool succeeded, IReadOnlyList<string> errors)
    {
        Succeeded = succeeded;
        Errors = errors;
    }

    public bool Succeeded { get; }

    public IReadOnlyList<string> Errors { get; }

    public static RmaOperationResult Success()
    {
        return new RmaOperationResult(true, []);
    }

    public static RmaOperationResult Failure(params string[] errors)
    {
        return new RmaOperationResult(false, errors);
    }

    public static RmaOperationResult Failure(IEnumerable<string> errors)
    {
        return new RmaOperationResult(false, errors.ToArray());
    }
}
