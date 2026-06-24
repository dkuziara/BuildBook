namespace BuildBook.Application.BuildRecords;

public sealed class UpdateHardwareDetailsResult
{
    private UpdateHardwareDetailsResult(
        bool succeeded,
        IReadOnlyList<string> errors,
        IReadOnlyList<string> warnings)
    {
        Succeeded = succeeded;
        Errors = errors;
        Warnings = warnings;
    }

    public bool Succeeded { get; }

    public IReadOnlyList<string> Errors { get; }

    public IReadOnlyList<string> Warnings { get; }

    public static UpdateHardwareDetailsResult Success(params string[] warnings)
    {
        return new UpdateHardwareDetailsResult(true, [], warnings);
    }

    public static UpdateHardwareDetailsResult Failure(params string[] errors)
    {
        return new UpdateHardwareDetailsResult(false, errors, []);
    }
}
