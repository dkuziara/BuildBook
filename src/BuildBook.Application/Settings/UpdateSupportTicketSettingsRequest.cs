namespace BuildBook.Application.Settings;

public sealed class UpdateSupportTicketSettingsRequest
{
    public string? SupportTicketLabel { get; set; }

    public string? SupportTicketUrlTemplate { get; set; }
}
