using BuildBook.Application.Security;

namespace BuildBook.Tests;

public class BuildBookPageAuthorizationTests
{
    [Theory]
    [InlineData("Home.razor", BuildBookPolicies.ViewBuildRecords)]
    [InlineData("BuildRegister.razor", BuildBookPolicies.ViewBuildRecords)]
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
            BuildBookPolicies.ExportNonSensitiveData => nameof(BuildBookPolicies.ExportNonSensitiveData),
            BuildBookPolicies.ManageUsers => nameof(BuildBookPolicies.ManageUsers),
            _ => throw new ArgumentOutOfRangeException(nameof(policy), policy, "No constant-name assertion is configured for this policy.")
        };
    }
}
