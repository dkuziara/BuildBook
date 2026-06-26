namespace BuildBook.Application.Security;

public sealed class ApplicationUserCommandResult
{
    private ApplicationUserCommandResult(bool succeeded, IReadOnlyList<string> errors)
    {
        Succeeded = succeeded;
        Errors = errors;
    }

    public bool Succeeded { get; }

    public IReadOnlyList<string> Errors { get; }

    public static ApplicationUserCommandResult Success()
    {
        return new ApplicationUserCommandResult(true, []);
    }

    public static ApplicationUserCommandResult Failure(params string[] errors)
    {
        return new ApplicationUserCommandResult(false, errors);
    }

    public static ApplicationUserCommandResult Failure(IReadOnlyList<string> errors)
    {
        return new ApplicationUserCommandResult(false, errors);
    }
}
