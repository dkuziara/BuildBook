namespace BuildBook.Application.Security;

public sealed record CreateApplicationUserRequest(
    string WindowsUserName,
    string? DisplayName,
    string? EmailAddress);
