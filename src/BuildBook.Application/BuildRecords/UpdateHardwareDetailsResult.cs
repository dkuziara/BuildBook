namespace BuildBook.Application.BuildRecords;

public sealed class UpdateHardwareDetailsResult
{
    private UpdateHardwareDetailsResult(bool succeeded, IReadOnlyList<string> errors)
    {
        Succeeded = succeeded;
        Errors = errors;
    }

    public bool Succeeded { get; }

    public IReadOnlyList<string> Errors { get; }

    public static UpdateHardwareDetailsResult Success()
    {
        return new UpdateHardwareDetailsResult(true, []);
    }

    public static UpdateHardwareDetailsResult Failure(params string[] errors)
    {
        return new UpdateHardwareDetailsResult(false, errors);
    }
}
