namespace BuildBook.Application.Security;

public sealed record ApplicationUserSummary(
    int Id,
    string WindowsUserName,
    string? DisplayName,
    string? EmailAddress,
    bool IsActive,
    bool IsBootstrapAdministrator,
    IReadOnlyList<string> AssignedRoles);
