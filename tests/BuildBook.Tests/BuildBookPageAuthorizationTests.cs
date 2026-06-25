using BuildBook.Application.Security;

namespace BuildBook.Tests;

public class BuildBookPageAuthorizationTests
{
    [Theory]
    [InlineData("Home.razor", BuildBookPolicies.ViewBuildRecords)]
    [InlineData("BuildRegister.razor", BuildBookPolicies.ViewBuildRecords)]
    [InlineData("BuildRecordDetail.razor", BuildBookPolicies.ViewBuildRecords)]
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
        Assert.Contains("NavigationManager.NavigateTo($\"/build-records/{result.BuildRecordId}\")", pageContent);
        Assert.Contains("Product code", pageContent);
        Assert.Contains("Product name", pageContent);
        Assert.Contains("Serial number", pageContent);
    }

    [Fact]
    public void BuildRegisterPageDefinesExpectedTable()
    {
        var pageContent = File.ReadAllText(GetPagePath("BuildRegister.razor"));

        Assert.Contains("IBuildRegisterReader", pageContent);
        Assert.Contains("FormName=\"build-register-filters\"", pageContent);
        Assert.Contains("Apply filters", pageContent);
        Assert.Contains("Clear filters", pageContent);
        Assert.Contains("SortByAsync(BuildRegisterSortColumn.ProductCode)", pageContent);
        Assert.Contains("SortByAsync(BuildRegisterSortColumn.LastUpdated)", pageContent);
        Assert.Contains("SortIndicator(BuildRegisterSortColumn.ProductCode)", pageContent);
        Assert.Contains("Product code", pageContent);
        Assert.Contains("Product name", pageContent);
        Assert.Contains("Serial number", pageContent);
        Assert.Contains("Customer", pageContent);
        Assert.Contains("Machine name", pageContent);
        Assert.Contains("RadSight version", pageContent);
        Assert.Contains("Windows version", pageContent);
        Assert.Contains("Date shipped", pageContent);
        Assert.Contains("Date assembled", pageContent);
        Assert.Contains("Date shipped", pageContent);
        Assert.Contains("Checked by", pageContent);
        Assert.Contains("Last updated", pageContent);
        Assert.Contains("/build-records/{buildRecord.Id}", pageContent);
        Assert.DoesNotContain("BuildRecordSecret", pageContent);
        Assert.DoesNotContain("Password", pageContent);
    }

    [Fact]
    public void BuildRecordDetailPageDefinesExpectedRouteAndSummary()
    {
        var pageContent = File.ReadAllText(GetPagePath("BuildRecordDetail.razor"));

        Assert.Contains("@page \"/build-records/{BuildRecordId:int}\"", pageContent);
        Assert.Contains("IBuildRecordDetailReader", pageContent);
        Assert.Contains("IProductDetailsUpdater", pageContent);
        Assert.Contains("IBuildDetailsUpdater", pageContent);
        Assert.Contains("ICustomerOptionsReader", pageContent);
        Assert.Contains("ICustomerShippingUpdater", pageContent);
        Assert.Contains("IHardwareDetailsUpdater", pageContent);
        Assert.Contains("ISoftwareFirmwareUpdater", pageContent);
        Assert.Contains("INetworkNotesUpdater", pageContent);
        Assert.Contains("FormName=\"edit-product-details\"", pageContent);
        Assert.Contains("FormName=\"edit-build-details\"", pageContent);
        Assert.Contains("FormName=\"edit-customer-shipping\"", pageContent);
        Assert.Contains("FormName=\"edit-hardware-details\"", pageContent);
        Assert.Contains("FormName=\"edit-software-firmware\"", pageContent);
        Assert.Contains("FormName=\"edit-network-details\"", pageContent);
        Assert.Contains("FormName=\"edit-notes\"", pageContent);
        Assert.Contains("Product Details", pageContent);
        Assert.Contains("Build Details", pageContent);
        Assert.Contains("Customer &amp; Shipping", pageContent);
        Assert.Contains("Hardware", pageContent);
        Assert.Contains("Software &amp; Firmware", pageContent);
        Assert.Contains("Network", pageContent);
        Assert.Contains("Notes", pageContent);
        Assert.Contains("Product code", pageContent);
        Assert.Contains("Product name", pageContent);
        Assert.Contains("Classification", pageContent);
        Assert.Contains("Serial number", pageContent);
        Assert.Contains("Status", pageContent);
        Assert.Contains("Assembled in", pageContent);
        Assert.Contains("Assembled by", pageContent);
        Assert.Contains("Date assembled", pageContent);
        Assert.Contains("H/W manufacturer", pageContent);
        Assert.Contains("Manufacturer part no.", pageContent);
        Assert.Contains("Manufacturer revision", pageContent);
        Assert.Contains("Manufacturer serial no.", pageContent);
        Assert.Contains("Packing list", pageContent);
        Assert.Contains("Checked by", pageContent);
        Assert.Contains("Customer", pageContent);
        Assert.Contains("Customer order", pageContent);
        Assert.Contains("OA number", pageContent);
        Assert.Contains("Invoice number", pageContent);
        Assert.Contains("Date shipped", pageContent);
        Assert.Contains("Panel device model", pageContent);
        Assert.Contains("Panel device serial", pageContent);
        Assert.Contains("Panel firmware version", pageContent);
        Assert.Contains("Machine name", pageContent);
        Assert.Contains("Radio serial number", pageContent);
        Assert.Contains("Router used", pageContent);
        Assert.Contains("Hardware notes", pageContent);
        Assert.Contains("Wi-Fi SSID", pageContent);
        Assert.Contains("Note", pageContent);
        Assert.Contains("Saved with warnings.", pageContent);
        Assert.Contains("saveWarnings", pageContent);
        Assert.Contains("Disk image version", pageContent);
        Assert.Contains("RadSight version", pageContent);
        Assert.Contains("Windows version", pageContent);
        Assert.Contains("Windows latest patch", pageContent);
        Assert.Contains("Bleuvio firmware version", pageContent);
        Assert.Contains("Charthouse IRDA firmware version", pageContent);
        Assert.Contains("Radio firmware", pageContent);
        Assert.DoesNotContain("BuildRecordSecret", pageContent);
        Assert.DoesNotContain("WifiPassword", pageContent);
        Assert.DoesNotContain("RouterPassword", pageContent);
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
