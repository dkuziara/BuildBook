using BuildBook.Application.Security;

namespace BuildBook.Tests;

public class BuildBookPageAuthorizationTests
{
    [Theory]
    [InlineData("Home.razor", BuildBookPolicies.ViewBuildRecords)]
    [InlineData("BuildRegister.razor", BuildBookPolicies.ViewBuildRecords)]
    [InlineData("CreateBuildRecord.razor", BuildBookPolicies.EditBuildRecords)]
    [InlineData("Reports.razor", BuildBookPolicies.ExportNonSensitiveData)]
    [InlineData("Admin.razor", BuildBookPolicies.ManageUsers)]
    public void PagesDeclareExpectedAuthorizationPolicy(string pageFileName, string expectedPolicy)
    {
        var pageContent = File.ReadAllText(GetPagePath(pageFileName));

        Assert.Contains($"@attribute [Authorize(Policy = BuildBookPolicies.{GetPolicyConstantName(expectedPolicy)})]", pageContent);
    }

    [Fact]
    public void MainNavigationUsesPageAuthorizationPolicies()
    {
        var layoutPath = Path.Combine(
            GetRepositoryRoot(),
            "src",
            "BuildBook.Web",
            "Components",
            "Layout",
            "MainLayout.razor");
        var layoutContent = File.ReadAllText(layoutPath);

        Assert.Contains($"Policy=\"@BuildBookPolicies.{nameof(BuildBookPolicies.ViewBuildRecords)}\"", layoutContent);
        Assert.Contains($"Policy=\"@BuildBookPolicies.{nameof(BuildBookPolicies.ExportNonSensitiveData)}\"", layoutContent);
        Assert.Contains($"Policy=\"@BuildBookPolicies.{nameof(BuildBookPolicies.ManageUsers)}\"", layoutContent);
    }

    [Fact]
    public void CreateBuildRecordPageDefinesExpectedRouteAndForm()
    {
        var pageContent = File.ReadAllText(GetPagePath("CreateBuildRecord.razor"));

        Assert.Contains("@page \"/build-records/new\"", pageContent);
        Assert.Contains("FormName=\"create-build-record\"", pageContent);
        Assert.Contains("Product code", pageContent);
        Assert.Contains("Product name", pageContent);
        Assert.Contains("Serial number", pageContent);
    }

    [Fact]
    public void AccessDeniedPageIsAvailableToSignedInUsers()
    {
        var pageContent = File.ReadAllText(GetPagePath("AccessDenied.razor"));

        Assert.Contains("@page \"/access-denied\"", pageContent);
        Assert.Contains("@attribute [Authorize]", pageContent);
        Assert.Contains("<AccessDeniedPanel />", pageContent);
    }

    [Fact]
    public void RoutesShowAccessDeniedForAuthenticatedUnauthorizedUsers()
    {
        var routesPath = Path.Combine(
            GetRepositoryRoot(),
            "src",
            "BuildBook.Web",
            "Components",
            "Routes.razor");
        var routesContent = File.ReadAllText(routesPath);

        Assert.Contains("<NotAuthorized Context=\"authenticationState\">", routesContent);
        Assert.Contains("authenticationState.User.Identity?.IsAuthenticated == true", routesContent);
        Assert.Contains("<AccessDeniedPanel />", routesContent);
        Assert.Contains("Sign in required", routesContent);
    }

    private static string GetPagePath(string pageFileName)
    {
        return Path.Combine(
            GetRepositoryRoot(),
            "src",
            "BuildBook.Web",
            "Components",
            "Pages",
            pageFileName);
    }

    private static string GetRepositoryRoot()
    {
        return Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
    }

    private static string GetPolicyConstantName(string policy)
    {
        return policy switch
        {
            BuildBookPolicies.ViewBuildRecords => nameof(BuildBookPolicies.ViewBuildRecords),
            BuildBookPolicies.EditBuildRecords => nameof(BuildBookPolicies.EditBuildRecords),
            BuildBookPolicies.ExportNonSensitiveData => nameof(BuildBookPolicies.ExportNonSensitiveData),
            BuildBookPolicies.ManageUsers => nameof(BuildBookPolicies.ManageUsers),
            _ => throw new ArgumentOutOfRangeException(nameof(policy), policy, "No constant-name assertion is configured for this policy.")
        };
    }
}
