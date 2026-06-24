namespace BuildBook.Application.BuildRecords;

public sealed class UpdateBuildDetailsResult
{
    private UpdateBuildDetailsResult(bool succeeded, IReadOnlyList<string> errors)
    {
        Succeeded = succeeded;
        Errors = errors;
    }

    public bool Succeeded { get; }

    public IReadOnlyList<string> Errors { get; }

    public static UpdateBuildDetailsResult Success()
    {
        return new UpdateBuildDetailsResult(true, []);
    }

    public static UpdateBuildDetailsResult Failure(params string[] errors)
    {
        return new UpdateBuildDetailsResult(false, errors);
    }
}
