using BuildBook.Domain.Rmas;

namespace BuildBook.Application.Rmas;

public sealed class ChangeRmaStatusResult
{
    private ChangeRmaStatusResult(
        bool succeeded,
        bool requiresConfirmation,
        IReadOnlyList<string> errors,
        IReadOnlyList<string> warnings)
    {
        Succeeded = succeeded;
        RequiresConfirmation = requiresConfirmation;
        Errors = errors;
        Warnings = warnings;
    }

    public bool Succeeded { get; }

    public bool RequiresConfirmation { get; }

    public IReadOnlyList<string> Errors { get; }

    public IReadOnlyList<string> Warnings { get; }

    public static ChangeRmaStatusResult Success()
    {
        return new ChangeRmaStatusResult(true, false, [], []);
    }

    public static ChangeRmaStatusResult Failure(params string[] errors)
    {
        return new ChangeRmaStatusResult(false, false, errors, []);
    }

    public static ChangeRmaStatusResult WarningConfirmationRequired(IReadOnlyList<string> warnings)
    {
        return new ChangeRmaStatusResult(false, true, [], warnings);
    }
}
