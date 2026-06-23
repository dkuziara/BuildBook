namespace BuildBook.Web.Configuration;

public sealed class BuildBookOptions
{
    public const string SectionName = "BuildBook";

    public string ApplicationName { get; init; } = "BuildBook";

    public string SupportContact { get; init; } = "Internal IT";

    public bool EnableDetailedErrors { get; init; }

    public int DefaultPageSize { get; init; } = 25;

    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(ApplicationName)
            && !string.IsNullOrWhiteSpace(SupportContact)
            && DefaultPageSize is >= 10 and <= 100;
    }
}
