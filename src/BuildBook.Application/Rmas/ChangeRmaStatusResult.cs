using BuildBook.Domain.Rmas;

namespace BuildBook.Application.Rmas;

public sealed class ChangeRmaStatusResult
{
    private ChangeRmaStatusResult(bool succeeded, IReadOnlyList<string> errors)
    {
        Succeeded = succeeded;
        Errors = errors;
    }

    public bool Succeeded { get; }

    public IReadOnlyList<string> Errors { get; }

    public static ChangeRmaStatusResult Success()
    {
        return new ChangeRmaStatusResult(true, []);
    }

    public static ChangeRmaStatusResult Failure(params string[] errors)
    {
        return new ChangeRmaStatusResult(false, errors);
    }
}
