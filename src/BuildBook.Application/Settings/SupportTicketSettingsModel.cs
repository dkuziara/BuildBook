namespace BuildBook.Application.Settings;

public sealed record SupportTicketSettingsModel(
    string SupportTicketLabel,
    string? SupportTicketUrlTemplate);
