using System.Security.Claims;
using BuildBook.Application.Security;
using BuildBook.Web.Authorization;
using BuildBook.Web.Configuration;

namespace BuildBook.Tests;

public class BuildBookRoleClaimsTransformationTests
{
    [Fact]
    public async Task TransformAsyncAddsResolvedBuildBookRoles()
    {
        var transformation = new BuildBookRoleClaimsTransformation(
            new StubRoleResolver([BuildBookRoles.Administrator, BuildBookRoles.Editor]));
        var principal = CreateAuthenticatedUser("AzureAD\\DavidKuziara");

        var transformedPrincipal = await transformation.TransformAsync(principal);

        Assert.True(transformedPrincipal.IsInRole(BuildBookRoles.Administrator));
        Assert.True(transformedPrincipal.IsInRole(BuildBookRoles.Editor));
    }

    [Fact]
    public async Task TransformAsyncReplacesExistingBuildBookRoleClaims()
    {
        var transformation = new BuildBookRoleClaimsTransformation(new StubRoleResolver([BuildBookRoles.Viewer]));
        var principal = CreateAuthenticatedUser("AzureAD\\DavidKuziara", BuildBookRoles.Administrator);

        var transformedPrincipal = await transformation.TransformAsync(principal);

        Assert.True(transformedPrincipal.IsInRole(BuildBookRoles.Viewer));
        Assert.False(transformedPrincipal.IsInRole(BuildBookRoles.Administrator));
    }

    [Fact]
    public async Task TransformAsyncSkipsAnonymousUsers()
    {
        var transformation = new BuildBookRoleClaimsTransformation(new StubRoleResolver([BuildBookRoles.Administrator]));
        var principal = new ClaimsPrincipal(new ClaimsIdentity());

        var transformedPrincipal = await transformation.TransformAsync(principal);

        Assert.False(transformedPrincipal.IsInRole(BuildBookRoles.Administrator));
    }

    [Fact]
    public void OptionsRejectBlankBootstrapAdministratorNames()
    {
        var options = new BuildBookOptions
        {
            Authorization = new BuildBookAuthorizationOptions
            {
                BootstrapAdministrators = ["AzureAD\\DavidKuziara", ""]
            }
        };

        Assert.False(options.IsValid());
    }

    private static ClaimsPrincipal CreateAuthenticatedUser(string userName, params string[] roles)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, userName)
        };
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        return new ClaimsPrincipal(new ClaimsIdentity(claims, authenticationType: "Test"));
    }

    private sealed class StubRoleResolver(IReadOnlyCollection<string> roles) : IBuildBookRoleResolver
    {
        public Task<IReadOnlyCollection<string>> GetEffectiveRolesAsync(
            string windowsUserName,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(roles);
        }
    }
}
