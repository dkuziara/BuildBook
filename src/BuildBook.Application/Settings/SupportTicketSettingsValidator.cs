namespace BuildBook.Application.Settings;

public static class SupportTicketSettingsValidator
{
    public const string DefaultSupportTicketLabel = "Support Ticket No.";

    public static IReadOnlyList<string> Validate(UpdateSupportTicketSettingsRequest request)
    {
        var errors = new List<string>();
        var label = NormalizeOptionalValue(request.SupportTicketLabel);
        var template = NormalizeOptionalValue(request.SupportTicketUrlTemplate);

        if (label is not null && label.Length > 128)
        {
            errors.Add("Support ticket label must be 128 characters or fewer.");
        }

        if (template is null)
        {
            return errors;
        }

        if (!template.Contains("{1}", StringComparison.Ordinal))
        {
            errors.Add("Support site URL template must contain {1}.");
        }

        if (!Uri.TryCreate(template, UriKind.Absolute, out var uri))
        {
            errors.Add("Support site URL template must be a valid absolute URL.");
            return errors;
        }

        if (!string.Equals(uri.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase)
            && !string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
        {
            errors.Add("Support site URL template must start with http:// or https://.");
        }

        return errors;
    }

    public static string GetDisplayLabel(string? configuredLabel)
    {
        return NormalizeOptionalValue(configuredLabel) ?? DefaultSupportTicketLabel;
    }

    public static string? NormalizeOptionalValue(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
