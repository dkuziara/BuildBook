namespace BuildBook.Application.Orders;

public sealed class ChangeOrderStatusResult
{
    private ChangeOrderStatusResult(
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

    public static ChangeOrderStatusResult Success() => new(true, false, [], []);

    public static ChangeOrderStatusResult Failure(params string[] errors) => new(false, false, errors, []);

    public static ChangeOrderStatusResult WarningConfirmationRequired(IReadOnlyList<string> warnings) => new(false, true, [], warnings);
}
