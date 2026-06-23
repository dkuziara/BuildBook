using BuildBook.Application.Security;

namespace BuildBook.Web.Configuration;

public sealed class BuildBookOptions
{
    public const string SectionName = "BuildBook";

    public string ApplicationName { get; init; } = "BuildBook";

    public string SupportContact { get; init; } = "Internal IT";

    public bool EnableDetailedErrors { get; init; }

    public bool SeedDevelopmentData { get; init; }

    public int DefaultPageSize { get; init; } = 25;

    public BuildBookAuthorizationOptions Authorization { get; init; } = new();

    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(ApplicationName)
            && !string.IsNullOrWhiteSpace(SupportContact)
            && DefaultPageSize is >= 10 and <= 100
            && Authorization.IsValid();
    }
}

public sealed class BuildBookAuthorizationOptions
{
    public bool UseDevelopmentAuthentication { get; init; }

    public string? DevelopmentRole { get; init; }

    public bool IsValid()
    {
        return string.IsNullOrWhiteSpace(DevelopmentRole)
            || BuildBookRoles.All.Contains(DevelopmentRole, StringComparer.Ordinal);
    }
}
