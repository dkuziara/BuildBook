namespace BuildBook.Application.Rmas;

public sealed class UpdateRmaWorkflowResult
{
    private UpdateRmaWorkflowResult(bool succeeded, IReadOnlyList<string> errors)
    {
        Succeeded = succeeded;
        Errors = errors;
    }

    public bool Succeeded { get; }

    public IReadOnlyList<string> Errors { get; }

    public static UpdateRmaWorkflowResult Success()
    {
        return new UpdateRmaWorkflowResult(true, []);
    }

    public static UpdateRmaWorkflowResult Failure(params string[] errors)
    {
        return new UpdateRmaWorkflowResult(false, errors);
    }
}
