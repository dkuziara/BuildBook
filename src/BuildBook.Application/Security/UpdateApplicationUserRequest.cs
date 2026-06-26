namespace BuildBook.Application.Security;

public sealed record UpdateApplicationUserRequest(
    int UserId,
    string? DisplayName,
    string? EmailAddress,
    bool IsActive,
    IReadOnlyCollection<string> AssignedRoles);
