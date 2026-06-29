namespace BuildBook.Application.Rmas;

public sealed class RmaLinkResult
{
    private RmaLinkResult(bool succeeded, IReadOnlyList<string> errors)
    {
        Succeeded = succeeded;
        Errors = errors;
    }

    public bool Succeeded { get; }

    public IReadOnlyList<string> Errors { get; }

    public static RmaLinkResult Success()
    {
        return new RmaLinkResult(true, []);
    }

    public static RmaLinkResult Failure(params string[] errors)
    {
        return new RmaLinkResult(false, errors);
    }
}
