namespace BuildBook.Application.Settings;

public interface ISystemSettingsService
{
    Task<SupportTicketSettingsModel> GetSupportTicketSettingsAsync(CancellationToken cancellationToken = default);

    Task<SupportTicketSettingsSaveResult> UpdateSupportTicketSettingsAsync(
        UpdateSupportTicketSettingsRequest request,
        string updatedBy,
        CancellationToken cancellationToken = default);
}
