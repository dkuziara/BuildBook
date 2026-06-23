using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace BuildBook.Web.Authorization;

public sealed class BuildBookPermissionService(IAuthorizationService authorizationService) : IBuildBookPermissionService
{
    public async Task<bool> IsAuthorizedAsync(
        ClaimsPrincipal user,
        string policyName,
        object? resource = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(user);
        ArgumentException.ThrowIfNullOrWhiteSpace(policyName);

        var authorizationResult = await authorizationService.AuthorizeAsync(user, resource, policyName);

        return authorizationResult.Succeeded;
    }

    public async Task EnsureAuthorizedAsync(
        ClaimsPrincipal user,
        string policyName,
        object? resource = null,
        CancellationToken cancellationToken = default)
    {
        if (await IsAuthorizedAsync(user, policyName, resource, cancellationToken))
        {
            return;
        }

        throw new UnauthorizedAccessException($"The current user is not authorized for policy '{policyName}'.");
    }
}
