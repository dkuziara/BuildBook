using System.Security.Claims;
using BuildBook.Application.Security;
using Microsoft.AspNetCore.Authentication;

namespace BuildBook.Web.Authorization;

public sealed class BuildBookRoleClaimsTransformation(
    IBuildBookRoleResolver roleResolver) : IClaimsTransformation
{
    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        if (principal.Identity?.IsAuthenticated != true || principal.Identity is not ClaimsIdentity identity)
        {
            return principal;
        }

        var windowsUserName = principal.Identity.Name;
        if (string.IsNullOrWhiteSpace(windowsUserName))
        {
            return principal;
        }

        foreach (var claim in identity.FindAll(ClaimTypes.Role).Where(claim => BuildBookRoles.All.Contains(claim.Value, StringComparer.Ordinal)).ToArray())
        {
            identity.RemoveClaim(claim);
        }

        var effectiveRoles = await roleResolver.GetEffectiveRolesAsync(windowsUserName);
        foreach (var roleName in effectiveRoles)
        {
            identity.AddClaim(new Claim(ClaimTypes.Role, roleName));
        }

        return principal;
    }
}
