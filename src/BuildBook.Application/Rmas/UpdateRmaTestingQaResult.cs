namespace BuildBook.Application.Rmas;

public sealed class UpdateRmaTestingQaResult
{
    private UpdateRmaTestingQaResult(bool succeeded, IReadOnlyList<string> errors)
    {
        Succeeded = succeeded;
        Errors = errors;
    }

    public bool Succeeded { get; }

    public IReadOnlyList<string> Errors { get; }

    public static UpdateRmaTestingQaResult Success() => new(true, []);

    public static UpdateRmaTestingQaResult Failure(params string[] errors) => new(false, errors);

    public static UpdateRmaTestingQaResult Failure(IReadOnlyList<string> errors) => new(false, errors);
}
