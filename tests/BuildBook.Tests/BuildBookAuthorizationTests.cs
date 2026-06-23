using BuildBook.Application.Security;
using BuildBook.Web.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace BuildBook.Tests;

public class BuildBookAuthorizationTests
{
    [Fact]
    public void RolesMatchVersionOneSpecification()
    {
        Assert.Equal(
            [
                "Administrator",
                "Editor",
                "Viewer",
                "Sensitive Data Viewer"
            ],
            BuildBookRoles.All);
    }

    [Theory]
    [InlineData(BuildBookPolicies.ViewBuildRecords, BuildBookRoles.Viewer, BuildBookRoles.Editor, BuildBookRoles.Administrator)]
    [InlineData(BuildBookPolicies.EditBuildRecords, BuildBookRoles.Editor, BuildBookRoles.Administrator)]
    [InlineData(BuildBookPolicies.RevealSensitiveData, BuildBookRoles.SensitiveDataViewer, BuildBookRoles.Administrator)]
    [InlineData(BuildBookPolicies.ImportSpreadsheet, BuildBookRoles.Administrator)]
    [InlineData(BuildBookPolicies.ExportNonSensitiveData, BuildBookRoles.Viewer, BuildBookRoles.Editor, BuildBookRoles.Administrator)]
    [InlineData(BuildBookPolicies.ManageUsers, BuildBookRoles.Administrator)]
    [InlineData(BuildBookPolicies.DeleteRecords, BuildBookRoles.Administrator)]
    public async Task PoliciesRequireExpectedRoles(string policyName, params string[] expectedRoles)
    {
        using var provider = CreateServiceProvider();
        var policyProvider = provider.GetRequiredService<IAuthorizationPolicyProvider>();

        var policy = await policyProvider.GetPolicyAsync(policyName);

        Assert.NotNull(policy);
        Assert.Equal(expectedRoles, GetRequiredRoles(policy));
    }

    [Fact]
    public void FallbackPolicyRequiresAuthenticatedUser()
    {
        using var provider = CreateServiceProvider();
        var options = provider.GetRequiredService<IOptions<AuthorizationOptions>>().Value;

        Assert.NotNull(options.FallbackPolicy);
        Assert.Contains(
            options.FallbackPolicy.Requirements,
            requirement => requirement is DenyAnonymousAuthorizationRequirement);
    }

    private static ServiceProvider CreateServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddBuildBookAuthorization();

        return services.BuildServiceProvider();
    }

    private static string[] GetRequiredRoles(AuthorizationPolicy policy)
    {
        return policy.Requirements
            .OfType<RolesAuthorizationRequirement>()
            .Single()
            .AllowedRoles
            .ToArray();
    }
}
