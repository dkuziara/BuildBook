namespace BuildBook.Application.BuildRecords;

public sealed class BuildRecordSecretSaveResult
{
    private BuildRecordSecretSaveResult(bool succeeded, IReadOnlyList<string> errors)
    {
        Succeeded = succeeded;
        Errors = errors;
    }

    public bool Succeeded { get; }

    public IReadOnlyList<string> Errors { get; }

    public static BuildRecordSecretSaveResult Success()
    {
        return new BuildRecordSecretSaveResult(true, []);
    }

    public static BuildRecordSecretSaveResult Failure(params string[] errors)
    {
        return new BuildRecordSecretSaveResult(false, errors);
    }
}
