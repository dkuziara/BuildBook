using BuildBook.Application.Settings;
using BuildBook.Domain.Settings;
using Microsoft.EntityFrameworkCore;

namespace BuildBook.Infrastructure.Persistence.Settings;

public sealed class SystemSettingsService(IDbContextFactory<BuildBookDbContext> dbContextFactory) : ISystemSettingsService
{
    public async Task<SupportTicketSettingsModel> GetSupportTicketSettingsAsync(CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        var settings = await dbContext.SystemSettings
            .AsNoTracking()
            .Where(setting =>
                setting.Key == SystemSettingKeys.SupportTicketLabel
                || setting.Key == SystemSettingKeys.SupportTicketUrlTemplate)
            .ToDictionaryAsync(setting => setting.Key, setting => setting.Value, cancellationToken);

        settings.TryGetValue(SystemSettingKeys.SupportTicketLabel, out var configuredLabel);
        settings.TryGetValue(SystemSettingKeys.SupportTicketUrlTemplate, out var urlTemplate);

        return new SupportTicketSettingsModel(
            SupportTicketSettingsValidator.GetDisplayLabel(configuredLabel),
            SupportTicketSettingsValidator.NormalizeOptionalValue(urlTemplate));
    }

    public async Task<SupportTicketSettingsSaveResult> UpdateSupportTicketSettingsAsync(
        UpdateSupportTicketSettingsRequest request,
        string updatedBy,
        CancellationToken cancellationToken = default)
    {
        var validationErrors = SupportTicketSettingsValidator.Validate(request);
        if (validationErrors.Count > 0)
        {
            return SupportTicketSettingsSaveResult.Failure(validationErrors);
        }

        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var userName = NormalizeUserName(updatedBy);
        var now = DateTimeOffset.UtcNow;
        var label = SupportTicketSettingsValidator.NormalizeOptionalValue(request.SupportTicketLabel);
        var urlTemplate = SupportTicketSettingsValidator.NormalizeOptionalValue(request.SupportTicketUrlTemplate);

        var existingSettings = await dbContext.SystemSettings
            .Where(setting =>
                setting.Key == SystemSettingKeys.SupportTicketLabel
                || setting.Key == SystemSettingKeys.SupportTicketUrlTemplate)
            .ToDictionaryAsync(setting => setting.Key, cancellationToken);

        UpdateSetting(
            existingSettings,
            dbContext,
            SystemSettingKeys.SupportTicketLabel,
            label,
            "Optional display label for support tickets in the RMA module.",
            userName,
            now);
        UpdateSetting(
            existingSettings,
            dbContext,
            SystemSettingKeys.SupportTicketUrlTemplate,
            urlTemplate,
            "Template used to build support ticket links. Use {1} for the ticket number placeholder.",
            userName,
            now);

        await dbContext.SaveChangesAsync(cancellationToken);
        return SupportTicketSettingsSaveResult.Success();
    }

    private static void UpdateSetting(
        IReadOnlyDictionary<string, SystemSetting> existingSettings,
        BuildBookDbContext dbContext,
        string key,
        string? value,
        string description,
        string userName,
        DateTimeOffset updatedAt)
    {
        if (existingSettings.TryGetValue(key, out var setting))
        {
            setting.Value = value;
            setting.Description = description;
            setting.LastUpdatedAt = updatedAt;
            setting.LastUpdatedBy = userName;
            return;
        }

        dbContext.SystemSettings.Add(new SystemSetting
        {
            Key = key,
            Value = value,
            Description = description,
            LastUpdatedAt = updatedAt,
            LastUpdatedBy = userName
        });
    }

    private static string NormalizeUserName(string updatedBy)
    {
        return string.IsNullOrWhiteSpace(updatedBy) ? "Unknown" : updatedBy.Trim();
    }
}
