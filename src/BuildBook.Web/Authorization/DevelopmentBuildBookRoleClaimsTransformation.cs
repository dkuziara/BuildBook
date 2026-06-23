using System.Security.Claims;
using BuildBook.Application.Security;
using BuildBook.Web.Configuration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace BuildBook.Web.Authorization;

public sealed class DevelopmentBuildBookRoleClaimsTransformation(
    IHostEnvironment hostEnvironment,
    IOptions<BuildBookOptions> options) : IClaimsTransformation
{
    public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        if (!hostEnvironment.IsDevelopment()
            || principal.Identity?.IsAuthenticated != true
            || string.IsNullOrWhiteSpace(options.Value.Authorization.DevelopmentRole)
            || HasBuildBookRole(principal))
        {
            return Task.FromResult(principal);
        }

        if (principal.Identity is ClaimsIdentity identity)
        {
            identity.AddClaim(new Claim(ClaimTypes.Role, options.Value.Authorization.DevelopmentRole));
        }

        return Task.FromResult(principal);
    }

    private static bool HasBuildBookRole(ClaimsPrincipal principal)
    {
        return BuildBookRoles.All.Any(principal.IsInRole);
    }
}
