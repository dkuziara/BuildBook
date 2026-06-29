namespace BuildBook.Application.Rmas;

public sealed class UpdateRmaIntakeResult
{
    private UpdateRmaIntakeResult(bool succeeded, IReadOnlyList<string> errors)
    {
        Succeeded = succeeded;
        Errors = errors;
    }

    public bool Succeeded { get; }

    public IReadOnlyList<string> Errors { get; }

    public static UpdateRmaIntakeResult Success()
    {
        return new UpdateRmaIntakeResult(true, []);
    }

    public static UpdateRmaIntakeResult Failure(params string[] errors)
    {
        return new UpdateRmaIntakeResult(false, errors);
    }
}
