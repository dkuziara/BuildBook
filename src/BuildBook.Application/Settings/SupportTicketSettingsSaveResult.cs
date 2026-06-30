namespace BuildBook.Application.Settings;

public sealed class SupportTicketSettingsSaveResult
{
    private SupportTicketSettingsSaveResult(bool succeeded, IReadOnlyList<string> errors)
    {
        Succeeded = succeeded;
        Errors = errors;
    }

    public bool Succeeded { get; }

    public IReadOnlyList<string> Errors { get; }

    public static SupportTicketSettingsSaveResult Success()
    {
        return new SupportTicketSettingsSaveResult(true, []);
    }

    public static SupportTicketSettingsSaveResult Failure(params string[] errors)
    {
        return new SupportTicketSettingsSaveResult(false, errors);
    }

    public static SupportTicketSettingsSaveResult Failure(IReadOnlyList<string> errors)
    {
        return new SupportTicketSettingsSaveResult(false, errors);
    }
}
