namespace BuildBook.Application.Security;

public interface IApplicationUserManagementService
{
    Task<IReadOnlyList<ApplicationUserSummary>> ListAsync(CancellationToken cancellationToken = default);

    Task<ApplicationUserCommandResult> CreateAsync(
        CreateApplicationUserRequest request,
        string createdBy,
        CancellationToken cancellationToken = default);

    Task<ApplicationUserCommandResult> UpdateAsync(
        UpdateApplicationUserRequest request,
        string updatedBy,
        CancellationToken cancellationToken = default);
}
