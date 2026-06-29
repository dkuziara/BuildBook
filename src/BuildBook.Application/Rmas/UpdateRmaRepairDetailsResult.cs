namespace BuildBook.Application.Rmas;

public sealed class UpdateRmaRepairDetailsResult
{
    private UpdateRmaRepairDetailsResult(bool succeeded, IReadOnlyList<string> errors)
    {
        Succeeded = succeeded;
        Errors = errors;
    }

    public bool Succeeded { get; }

    public IReadOnlyList<string> Errors { get; }

    public static UpdateRmaRepairDetailsResult Success() => new(true, []);

    public static UpdateRmaRepairDetailsResult Failure(params string[] errors) => new(false, errors);

    public static UpdateRmaRepairDetailsResult Failure(IReadOnlyList<string> errors) => new(false, errors);
}
