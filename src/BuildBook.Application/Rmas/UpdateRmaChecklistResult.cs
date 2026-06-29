namespace BuildBook.Application.Rmas;

public sealed class UpdateRmaChecklistResult
{
    private UpdateRmaChecklistResult(bool succeeded, IReadOnlyList<string> errors)
    {
        Succeeded = succeeded;
        Errors = errors;
    }

    public bool Succeeded { get; }

    public IReadOnlyList<string> Errors { get; }

    public static UpdateRmaChecklistResult Success() => new(true, []);

    public static UpdateRmaChecklistResult Failure(params string[] errors) => new(false, errors);

    public static UpdateRmaChecklistResult Failure(IReadOnlyList<string> errors) => new(false, errors);
}
