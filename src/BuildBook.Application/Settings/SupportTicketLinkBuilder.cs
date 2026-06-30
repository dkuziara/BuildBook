namespace BuildBook.Application.Settings;

public static class SupportTicketLinkBuilder
{
    public static string? BuildUrl(string? template, string? ticketNumber)
    {
        var normalizedTemplate = SupportTicketSettingsValidator.NormalizeOptionalValue(template);
        var normalizedTicketNumber = SupportTicketSettingsValidator.NormalizeOptionalValue(ticketNumber);

        if (normalizedTemplate is null || normalizedTicketNumber is null)
        {
            return null;
        }

        if (!normalizedTemplate.Contains("{1}", StringComparison.Ordinal))
        {
            return null;
        }

        if (!Uri.TryCreate(normalizedTemplate, UriKind.Absolute, out var templateUri))
        {
            return null;
        }

        if (!string.Equals(templateUri.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase)
            && !string.Equals(templateUri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var encodedTicketNumber = Uri.EscapeDataString(normalizedTicketNumber);
        var url = normalizedTemplate.Replace("{1}", encodedTicketNumber, StringComparison.Ordinal);

        return Uri.TryCreate(url, UriKind.Absolute, out var finalUri)
            && (string.Equals(finalUri.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase)
                || string.Equals(finalUri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
            ? url
            : null;
    }
}
