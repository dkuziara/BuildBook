namespace BuildBook.Application.BuildRecords;

public sealed class UpdateSoftwareFirmwareResult
{
    private UpdateSoftwareFirmwareResult(bool succeeded, IReadOnlyList<string> errors)
    {
        Succeeded = succeeded;
        Errors = errors;
    }

    public bool Succeeded { get; }

    public IReadOnlyList<string> Errors { get; }

    public static UpdateSoftwareFirmwareResult Success()
    {
        return new UpdateSoftwareFirmwareResult(true, []);
    }

    public static UpdateSoftwareFirmwareResult Failure(params string[] errors)
    {
        return new UpdateSoftwareFirmwareResult(false, errors);
    }
}
