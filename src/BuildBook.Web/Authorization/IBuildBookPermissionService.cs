using System.Security.Claims;

namespace BuildBook.Web.Authorization;

public interface IBuildBookPermissionService
{
    Task<bool> IsAuthorizedAsync(
        ClaimsPrincipal user,
        string policyName,
        object? resource = null,
        CancellationToken cancellationToken = default);

    Task EnsureAuthorizedAsync(
        ClaimsPrincipal user,
        string policyName,
        object? resource = null,
        CancellationToken cancellationToken = default);
}
