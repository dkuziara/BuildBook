namespace BuildBook.Application.Rmas;

public sealed class UpdateRmaFaultDetailsResult
{
    private UpdateRmaFaultDetailsResult(bool succeeded, IReadOnlyList<string> errors)
    {
        Succeeded = succeeded;
        Errors = errors;
    }

    public bool Succeeded { get; }

    public IReadOnlyList<string> Errors { get; }

    public static UpdateRmaFaultDetailsResult Success() => new(true, []);

    public static UpdateRmaFaultDetailsResult Failure(params string[] errors) => new(false, errors);

    public static UpdateRmaFaultDetailsResult Failure(IReadOnlyList<string> errors) => new(false, errors);
}
