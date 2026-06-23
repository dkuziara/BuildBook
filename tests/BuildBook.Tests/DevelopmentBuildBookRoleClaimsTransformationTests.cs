using System.Security.Claims;
using BuildBook.Application.Security;
using BuildBook.Web.Authorization;
using BuildBook.Web.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace BuildBook.Tests;

public class DevelopmentBuildBookRoleClaimsTransformationTests
{
    [Fact]
    public async Task TransformAsyncAddsConfiguredRoleInDevelopment()
    {
        var transformation = CreateTransformation("Development", BuildBookRoles.Administrator);
        var principal = CreateAuthenticatedUser();

        var transformedPrincipal = await transformation.TransformAsync(principal);

        Assert.True(transformedPrincipal.IsInRole(BuildBookRoles.Administrator));
    }

    [Fact]
    public async Task TransformAsyncDoesNotAddConfiguredRoleOutsideDevelopment()
    {
        var transformation = CreateTransformation("Production", BuildBookRoles.Administrator);
        var principal = CreateAuthenticatedUser();

        var transformedPrincipal = await transformation.TransformAsync(principal);

        Assert.False(transformedPrincipal.IsInRole(BuildBookRoles.Administrator));
    }

    [Fact]
    public async Task TransformAsyncDoesNotReplaceExistingBuildBookRole()
    {
        var transformation = CreateTransformation("Development", BuildBookRoles.Administrator);
        var principal = CreateAuthenticatedUser(BuildBookRoles.Viewer);

        var transformedPrincipal = await transformation.TransformAsync(principal);

        Assert.True(transformedPrincipal.IsInRole(BuildBookRoles.Viewer));
        Assert.False(transformedPrincipal.IsInRole(BuildBookRoles.Administrator));
    }

    [Fact]
    public void OptionsRejectUnknownDevelopmentRole()
    {
        var options = new BuildBookOptions
        {
            Authorization = new BuildBookAuthorizationOptions
            {
                DevelopmentRole = "Unknown Role"
            }
        };

        Assert.False(options.IsValid());
    }

    private static DevelopmentBuildBookRoleClaimsTransformation CreateTransformation(
        string environmentName,
        string? developmentRole)
    {
        return new DevelopmentBuildBookRoleClaimsTransformation(
            new TestHostEnvironment(environmentName),
            Options.Create(new BuildBookOptions
            {
                Authorization = new BuildBookAuthorizationOptions
                {
                    DevelopmentRole = developmentRole
                }
            }));
    }

    private static ClaimsPrincipal CreateAuthenticatedUser(params string[] roles)
    {
        var claims = roles.Select(role => new Claim(ClaimTypes.Role, role));
        var identity = new ClaimsIdentity(claims, authenticationType: "Test");

        return new ClaimsPrincipal(identity);
    }

    private sealed class TestHostEnvironment(string environmentName) : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = environmentName;

        public string ApplicationName { get; set; } = "BuildBook.Tests";

        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;

        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
