using BuildBook.Application.Security;
using BuildBook.Web.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;

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
    [InlineData(BuildBookPolicies.ViewCustomers, BuildBookRoles.Viewer, BuildBookRoles.Editor, BuildBookRoles.Administrator)]
    [InlineData(BuildBookPolicies.AddCustomers, BuildBookRoles.Editor, BuildBookRoles.Administrator)]
    [InlineData(BuildBookPolicies.EditCustomers, BuildBookRoles.Editor, BuildBookRoles.Administrator)]
    [InlineData(BuildBookPolicies.ExportCustomers, BuildBookRoles.Viewer, BuildBookRoles.Editor, BuildBookRoles.Administrator)]
    [InlineData(BuildBookPolicies.ManageSupportContractLevels, BuildBookRoles.Administrator)]
    [InlineData(BuildBookPolicies.ManageSystemSettings, BuildBookRoles.Administrator)]
    [InlineData(BuildBookPolicies.RevealSensitiveData, BuildBookRoles.SensitiveDataViewer, BuildBookRoles.Administrator)]
    [InlineData(BuildBookPolicies.ImportSpreadsheet, BuildBookRoles.Administrator)]
    [InlineData(BuildBookPolicies.ExportNonSensitiveData, BuildBookRoles.Viewer, BuildBookRoles.Editor, BuildBookRoles.Administrator)]
    [InlineData(BuildBookPolicies.ManageUsers, BuildBookRoles.Administrator)]
    [InlineData(BuildBookPolicies.DeleteRecords, BuildBookRoles.Administrator)]
    [InlineData(BuildBookOrderPolicies.ViewOrders, BuildBookRoles.Viewer, BuildBookRoles.Editor, BuildBookRoles.Administrator)]
    [InlineData(BuildBookOrderPolicies.CreateOrders, BuildBookRoles.Editor, BuildBookRoles.Administrator)]
    [InlineData(BuildBookOrderPolicies.EditOrders, BuildBookRoles.Editor, BuildBookRoles.Administrator)]
    [InlineData(BuildBookOrderPolicies.ChangeOrderStatus, BuildBookRoles.Editor, BuildBookRoles.Administrator)]
    [InlineData(BuildBookOrderPolicies.CompleteOrderChecklist, BuildBookRoles.Editor, BuildBookRoles.Administrator)]
    [InlineData(BuildBookOrderPolicies.LinkOrderBuildRecords, BuildBookRoles.Editor, BuildBookRoles.Administrator)]
    [InlineData(BuildBookOrderPolicies.ImportOrders, BuildBookRoles.Administrator)]
    [InlineData(BuildBookOrderPolicies.ExportOrders, BuildBookRoles.Viewer, BuildBookRoles.Editor, BuildBookRoles.Administrator)]
    [InlineData(BuildBookOrderPolicies.DeleteOrders, BuildBookRoles.Administrator)]
    [InlineData(BuildBookRmaPolicies.ViewRmas, BuildBookRoles.Viewer, BuildBookRoles.Editor, BuildBookRoles.Administrator)]
    [InlineData(BuildBookRmaPolicies.CreateRmas, BuildBookRoles.Editor, BuildBookRoles.Administrator)]
    [InlineData(BuildBookRmaPolicies.EditRmas, BuildBookRoles.Editor, BuildBookRoles.Administrator)]
    [InlineData(BuildBookRmaPolicies.ChangeRmaStatus, BuildBookRoles.Editor, BuildBookRoles.Administrator)]
    [InlineData(BuildBookRmaPolicies.CloseRmas, BuildBookRoles.Editor, BuildBookRoles.Administrator)]
    [InlineData(BuildBookRmaPolicies.ExportRmaReports, BuildBookRoles.Viewer, BuildBookRoles.Editor, BuildBookRoles.Administrator)]
    [InlineData(BuildBookRmaPolicies.ManageRmaSettings, BuildBookRoles.Administrator)]
    [InlineData(BuildBookRmaPolicies.DeleteRmas, BuildBookRoles.Administrator)]
    public async Task PoliciesRequireExpectedRoles(string policyName, params string[] expectedRoles)
    {
        using var provider = CreateServiceProvider();
        var policyProvider = provider.GetRequiredService<IAuthorizationPolicyProvider>();

        var policy = await policyProvider.GetPolicyAsync(policyName);

        Assert.NotNull(policy);
        Assert.Equal(expectedRoles, GetRequiredRoles(policy));
    }

    [Theory]
    [InlineData(true, BuildBookRoles.Administrator)]
    [InlineData(true, BuildBookRoles.Editor, BuildBookRoles.SensitiveDataViewer)]
    [InlineData(false, BuildBookRoles.Editor)]
    [InlineData(false, BuildBookRoles.SensitiveDataViewer)]
    [InlineData(false, BuildBookRoles.Viewer)]
    public async Task ManageSensitiveDataPolicy_RequiresAdministratorOrEditorWithSensitiveDataViewer(
        bool expectedAuthorized,
        params string[] roles)
    {
        using var provider = CreateServiceProvider();
        var permissionService = provider.GetRequiredService<IBuildBookPermissionService>();
        var user = CreateUser(roles);

        var isAuthorized = await permissionService.IsAuthorizedAsync(user, BuildBookPolicies.ManageSensitiveData);

        Assert.Equal(expectedAuthorized, isAuthorized);
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

    [Fact]
    public async Task PermissionServiceAllowsUserWithRequiredRole()
    {
        using var provider = CreateServiceProvider();
        var permissionService = provider.GetRequiredService<IBuildBookPermissionService>();
        var user = CreateUser(BuildBookRoles.Editor);

        var isAuthorized = await permissionService.IsAuthorizedAsync(user, BuildBookPolicies.EditBuildRecords);

        Assert.True(isAuthorized);
    }

    [Fact]
    public async Task PermissionServiceRejectsUserWithoutRequiredRole()
    {
        using var provider = CreateServiceProvider();
        var permissionService = provider.GetRequiredService<IBuildBookPermissionService>();
        var user = CreateUser(BuildBookRoles.Viewer);

        var isAuthorized = await permissionService.IsAuthorizedAsync(user, BuildBookPolicies.EditBuildRecords);

        Assert.False(isAuthorized);
    }

    [Fact]
    public async Task EnsureAuthorizedThrowsWhenUserDoesNotMeetPolicy()
    {
        using var provider = CreateServiceProvider();
        var permissionService = provider.GetRequiredService<IBuildBookPermissionService>();
        var user = CreateUser(BuildBookRoles.Viewer);

        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => permissionService.EnsureAuthorizedAsync(user, BuildBookPolicies.RevealSensitiveData));

        Assert.Contains(BuildBookPolicies.RevealSensitiveData, exception.Message);
    }

    private static ServiceProvider CreateServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddLogging();
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

    private static ClaimsPrincipal CreateUser(params string[] roles)
    {
        var claims = roles.Select(role => new Claim(ClaimTypes.Role, role));
        var identity = new ClaimsIdentity(claims, authenticationType: "Test");

        return new ClaimsPrincipal(identity);
    }
}
