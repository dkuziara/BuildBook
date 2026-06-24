namespace BuildBook.Application.BuildRecords;

public sealed class UpdateProductDetailsResult
{
    private UpdateProductDetailsResult(bool succeeded, IReadOnlyList<string> errors)
    {
        Succeeded = succeeded;
        Errors = errors;
    }

    public bool Succeeded { get; }

    public IReadOnlyList<string> Errors { get; }

    public static UpdateProductDetailsResult Success()
    {
        return new UpdateProductDetailsResult(true, []);
    }

    public static UpdateProductDetailsResult Failure(params string[] errors)
    {
        return new UpdateProductDetailsResult(false, errors);
    }
}
