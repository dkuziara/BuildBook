namespace BuildBook.Application.BuildRecords;

public sealed class UpdateNetworkNotesResult
{
    private UpdateNetworkNotesResult(bool succeeded, IReadOnlyList<string> errors)
    {
        Succeeded = succeeded;
        Errors = errors;
    }

    public bool Succeeded { get; }

    public IReadOnlyList<string> Errors { get; }

    public static UpdateNetworkNotesResult Success()
    {
        return new UpdateNetworkNotesResult(true, []);
    }

    public static UpdateNetworkNotesResult Failure(params string[] errors)
    {
        return new UpdateNetworkNotesResult(false, errors);
    }
}
