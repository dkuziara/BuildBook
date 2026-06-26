using BuildBook.Application.Security;

namespace BuildBook.Tests;

public class BuildBookPageAuthorizationTests
{
    [Theory]
    [InlineData("Home.razor", BuildBookPolicies.ViewBuildRecords)]
    [InlineData("BuildRegister.razor", BuildBookPolicies.ViewBuildRecords)]
    [InlineData("BuildRecordDetail.razor", BuildBookPolicies.ViewBuildRecords)]
    [InlineData("CreateBuildRecord.razor", BuildBookPolicies.EditBuildRecords)]
    [InlineData("ImportSpreadsheet.razor", BuildBookPolicies.ImportSpreadsheet)]
    [InlineData("ImportHistory.razor", BuildBookPolicies.ImportSpreadsheet)]
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
        Assert.Contains("Signed in as", layoutContent);
        Assert.Contains("Windows Authentication", layoutContent);
        Assert.Contains("Local development authentication bypass", layoutContent);
    }

    [Fact]
    public void ImportSpreadsheetPageDefinesExpectedRouteAndUploadControls()
    {
        var pageContent = File.ReadAllText(GetPagePath("ImportSpreadsheet.razor"));

        Assert.Contains("@page \"/imports/upload\"", pageContent);
        Assert.Contains("@rendermode InteractiveServer", pageContent);
        Assert.Contains("Upload Spreadsheet", pageContent);
        Assert.Contains("InputFile", pageContent);
        Assert.Contains("HandleFileSelected", pageContent);
        Assert.Contains("UploadSpreadsheetAsync", pageContent);
        Assert.Contains("ISpreadsheetImportMappingService", pageContent);
        Assert.Contains("Column Mapping", pageContent);
        Assert.Contains("Import Preview", pageContent);
        Assert.Contains("Import Validation", pageContent);
        Assert.Contains("Import Summary", pageContent);
        Assert.Contains("SaveColumnMappingAsync", pageContent);
        Assert.Contains("BuildPreviewAsync", pageContent);
        Assert.Contains("ValidateImportAsync", pageContent);
        Assert.Contains("BuildValidationAsync", pageContent);
        Assert.Contains("RunImportAsync", pageContent);
        Assert.Contains("BuildImportAsync", pageContent);
        Assert.Contains("AuthenticationStateProvider", pageContent);
        Assert.Contains("importExecution", pageContent);
        Assert.Contains("importValidation", pageContent);
        Assert.Contains("importPreview", pageContent);
        Assert.Contains("DisplayPreviewValue", pageContent);
        Assert.Contains("selectedMappings", pageContent);
        Assert.Contains("DescribeMapping", pageContent);
        Assert.Contains("accept=\".xlsx,.xls,.csv\"", pageContent);
        Assert.Contains("MaximumUploadBytes", pageContent);
        Assert.Contains("AllowedExtensions", pageContent);
        Assert.Contains("Choose an Excel workbook or CSV file.", pageContent);
        Assert.Contains("The spreadsheet must be 25 MB or smaller.", pageContent);
        Assert.Contains("Import History", pageContent);
        Assert.Contains("href=\"/imports\"", pageContent);
        Assert.Contains("must be mapped before import can continue.", pageContent);
        Assert.Contains("Stored separately, encrypted and excluded from normal search and exports.", pageContent);
        Assert.Contains("Review a sample of the mapped spreadsheet rows before moving on to validation.", pageContent);
        Assert.Contains("Rows checked", pageContent);
        Assert.Contains("Severity", pageContent);
        Assert.Contains("No validation issues were found in the previewed import data.", pageContent);
        Assert.Contains("Records created", pageContent);
        Assert.Contains("Records skipped", pageContent);
        Assert.Contains("Normal Build Record fields have been converted into BuildBook records.", pageContent);
    }

    [Fact]
    public void ImportHistoryPageDefinesExpectedRouteAndSummaryTable()
    {
        var pageContent = File.ReadAllText(GetPagePath("ImportHistory.razor"));

        Assert.Contains("@page \"/imports\"", pageContent);
        Assert.Contains("IImportHistoryReader", pageContent);
        Assert.Contains("Import History", pageContent);
        Assert.Contains("Import Summary", pageContent);
        Assert.Contains("Total batches", pageContent);
        Assert.Contains("Records created", pageContent);
        Assert.Contains("Warnings", pageContent);
        Assert.Contains("Errors", pageContent);
        Assert.Contains("History", pageContent);
        Assert.Contains("Source file", pageContent);
        Assert.Contains("Imported by", pageContent);
        Assert.Contains("Rows read", pageContent);
        Assert.Contains("Created", pageContent);
        Assert.Contains("Skipped", pageContent);
        Assert.Contains("DisplayStatus", pageContent);
        Assert.Contains("FormatTimestamp", pageContent);
        Assert.Contains("Open Upload Screen", pageContent);
        Assert.Contains("Back to Admin", pageContent);
        Assert.Contains("No spreadsheet imports have been run yet.", pageContent);
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
    public void HomePageDefinesRecentActivityPanels()
    {
        var pageContent = File.ReadAllText(GetPagePath("Home.razor"));

        Assert.Contains("IHomePageReader", pageContent);
        Assert.Contains("AuthenticationStateProvider", pageContent);
        Assert.Contains("Recently viewed", pageContent);
        Assert.Contains("Recently updated", pageContent);
        Assert.Contains("Build Records you opened recently.", pageContent);
        Assert.Contains("Build Records with the latest saved changes.", pageContent);
        Assert.Contains("/build-records/@record.Id", pageContent);
        Assert.DoesNotContain("BuildRecordSecrets", pageContent);
        Assert.DoesNotContain("Password", pageContent);
    }

    [Fact]
    public void BuildRegisterPageDefinesExpectedTable()
    {
        var pageContent = File.ReadAllText(GetPagePath("BuildRegister.razor"));

        Assert.Contains("IBuildRegisterReader", pageContent);
        Assert.Contains("FormName=\"build-register-filters\"", pageContent);
        Assert.Contains("Apply filters", pageContent);
        Assert.Contains("Clear filters", pageContent);
        Assert.Contains("canClearFilters", pageContent);
        Assert.Contains("BuildBookPolicies.ExportNonSensitiveData", pageContent);
        Assert.Contains("Export current results to CSV", pageContent);
        Assert.Contains("Export current results to Excel", pageContent);
        Assert.Contains("/reports/build-register.{format}", pageContent);
        Assert.Contains("BuildRegisterCsvDownloadUrl", pageContent);
        Assert.Contains("BuildRegisterExcelDownloadUrl", pageContent);
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
        Assert.DoesNotContain("BuildRecordSecrets", pageContent);
        Assert.DoesNotContain("Password", pageContent);
    }

    [Fact]
    public void ReportsPageLinksToBuildRegisterCsvExport()
    {
        var pageContent = File.ReadAllText(GetPagePath("Reports.razor"));

        Assert.Contains("@rendermode InteractiveServer", pageContent);
        Assert.Contains("IBuildRegisterReader", pageContent);
        Assert.Contains("ICustomerOptionsReader", pageContent);
        Assert.Contains("IMissingDataReportReader", pageContent);
        Assert.Contains("Export current Build Register results to CSV or Excel", pageContent);
        Assert.Contains("Current search results", pageContent);
        Assert.Contains("Devices by customer", pageContent);
        Assert.Contains("Missing data reports", pageContent);
        Assert.Contains("Missing customer", pageContent);
        Assert.Contains("Missing QA number", pageContent);
        Assert.Contains("Missing recovery key", pageContent);
        Assert.Contains("Missing shipped date", pageContent);
        Assert.Contains("SupplyParameterFromQuery", pageContent);
        Assert.Contains("MissingDataReportQuery", pageContent);
        Assert.Contains("MissingDataReportLink", pageContent);
        Assert.Contains("?missingData=", pageContent);
        Assert.Contains("#missing-data-records-heading", pageContent);
        Assert.Contains("report-scroll-heading", pageContent);
        Assert.Contains("missing-data-records-section", pageContent);
        Assert.Contains("report-scroll-target", pageContent);
        Assert.Contains("QA number reporting will be available once the field is added to Build Records.", pageContent);
        Assert.Contains("The missing data reports could not be loaded. Refresh the page and try again.", pageContent);
        Assert.Contains("Build Records missing customer", pageContent);
        Assert.Contains("Build Records missing recovery key", pageContent);
        Assert.Contains("Build Records missing shipped date", pageContent);
        Assert.Contains("No Build Records match this missing-data report.", pageContent);
        Assert.Contains("Software and version reports", pageContent);
        Assert.Contains("RadSight versions", pageContent);
        Assert.Contains("Windows versions", pageContent);
        Assert.Contains("BuildVersionReportRows", pageContent);
        Assert.Contains("BuildVersionReportLink", pageContent);
        Assert.Contains("The version reports could not be loaded. Refresh the page and try again.", pageContent);
        Assert.Contains("BuildVersionReportLink(\"radSightVersion\"", pageContent);
        Assert.Contains("BuildVersionReportLink(\"windowsVersion\"", pageContent);
        Assert.Contains("Open matching records", pageContent);
        Assert.Contains("No RadSight versions have been recorded yet.", pageContent);
        Assert.Contains("No Windows versions have been recorded yet.", pageContent);
        Assert.Contains("InputSelect", pageContent);
        Assert.Contains("Select customer", pageContent);
        Assert.Contains("Show report", pageContent);
        Assert.Contains("ClearCustomerReportAsync", pageContent);
        Assert.Contains("LoadCustomerReportAsync", pageContent);
        Assert.Contains("CustomerOptionsReader.ListActiveAsync", pageContent);
        Assert.Contains("BuildRegisterReader.ListAsync", pageContent);
        Assert.Contains("MissingDataReportReader.ListActiveAsync", pageContent);
        Assert.Contains("BuildRegisterFilter", pageContent);
        Assert.Contains("selectedCustomerId", pageContent);
        Assert.Contains("canRunReport", pageContent);
        Assert.Contains("reportError", pageContent);
        Assert.Contains("missingDataReportError", pageContent);
        Assert.Contains("versionReportError", pageContent);
        Assert.Contains("The customer report could not be loaded. Refresh the page and try again.", pageContent);
        Assert.Contains("No Build Records were found for the selected customer.", pageContent);
        Assert.Contains("Select a customer to list all matching Build Records.", pageContent);
        Assert.Contains("/build-records/{buildRecord.Id}", pageContent);
        Assert.Contains("/build-register", pageContent);
        Assert.DoesNotContain("Password", pageContent);
        Assert.DoesNotContain("BitLocker", pageContent);
    }

    [Fact]
    public void BuildRecordDetailPageDefinesExpectedRouteAndSummary()
    {
        var pageContent = File.ReadAllText(GetPagePath("BuildRecordDetail.razor"));

        Assert.Contains("@page \"/build-records/{BuildRecordId:int}\"", pageContent);
        Assert.Contains("IBuildRecordDetailReader", pageContent);
        Assert.Contains("IBuildRecordAuditHistoryReader", pageContent);
        Assert.Contains("IProductDetailsUpdater", pageContent);
        Assert.Contains("IBuildDetailsUpdater", pageContent);
        Assert.Contains("ICustomerOptionsReader", pageContent);
        Assert.Contains("ICustomerShippingUpdater", pageContent);
        Assert.Contains("IHardwareDetailsUpdater", pageContent);
        Assert.Contains("ISoftwareFirmwareUpdater", pageContent);
        Assert.Contains("INetworkNotesUpdater", pageContent);
        Assert.Contains("IBuildRecordSecretService", pageContent);
        Assert.Contains("IBuildBookPermissionService", pageContent);
        Assert.Contains("IRecentlyViewedBuildRecordTracker", pageContent);
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
        Assert.Contains("Credentials &amp; Recovery", pageContent);
        Assert.Contains("Notes", pageContent);
        Assert.Contains("History", pageContent);
        Assert.Contains("Audit history for this Build Record.", pageContent);
        Assert.Contains("Date/time", pageContent);
        Assert.Contains("Field changed", pageContent);
        Assert.Contains("Old value", pageContent);
        Assert.Contains("New value", pageContent);
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
        Assert.Contains("RadSight user login", pageContent);
        Assert.Contains("Kiosk user", pageContent);
        Assert.Contains("Windows admin user", pageContent);
        Assert.Contains("RadSight user password", pageContent);
        Assert.Contains("Windows admin password", pageContent);
        Assert.Contains("Kiosk password", pageContent);
        Assert.Contains("Wi-Fi password", pageContent);
        Assert.Contains("Router password", pageContent);
        Assert.Contains("BitLocker recovery key", pageContent);
        Assert.Contains("MaskedSecretDisplay", pageContent);
        Assert.Contains("************", pageContent);
        Assert.Contains("BuildBookPolicies.RevealSensitiveData", pageContent);
        Assert.Contains("BuildBookPolicies.ManageSensitiveData", pageContent);
        Assert.Contains("EnsureAuthorizedAsync", pageContent);
        Assert.Contains("BuildRecordSecretService.RetrieveAsync", pageContent);
        Assert.Contains("BuildRecordSecretService.SaveAsync", pageContent);
        Assert.Contains("BuildRecordSecretService.UpdateAsync", pageContent);
        Assert.Contains("RevealSecretAsync", pageContent);
        Assert.Contains("HideSecret", pageContent);
        Assert.Contains("SaveSecretAsync", pageContent);
        Assert.Contains("SupportedSecretTypes", pageContent);
        Assert.Contains("SecretTypesSet", pageContent);
        Assert.Contains("Hide", pageContent);
        Assert.Contains("Set", pageContent);
        Assert.Contains("Update", pageContent);
        Assert.Contains("Confirm value", pageContent);
        Assert.Contains("edit-secret-", pageContent);
        Assert.Contains("Use eight groups of six digits separated by hyphens.", pageContent);
        Assert.Contains("RecentlyViewedBuildRecordTracker.TrackView", pageContent);
        Assert.DoesNotContain("BuildRecordSecrets", pageContent);
        Assert.DoesNotContain("SecretValueEncrypted", pageContent);
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
    public void AdminPageSupportsUserAndRoleManagement()
    {
        var pageContent = File.ReadAllText(GetPagePath("Admin.razor"));

        Assert.Contains("@rendermode InteractiveServer", pageContent);
        Assert.Contains("IApplicationUserManagementService", pageContent);
        Assert.Contains("AuthenticationStateProvider", pageContent);
        Assert.Contains("Users &amp; Roles", pageContent);
        Assert.Contains("Windows Authentication identifies each person.", pageContent);
        Assert.Contains("BuildBook has no separate login form.", pageContent);
        Assert.Contains("Bootstrap administrators", pageContent);
        Assert.Contains("Add Windows user", pageContent);
        Assert.Contains("Windows username", pageContent);
        Assert.Contains("Display name", pageContent);
        Assert.Contains("Email address", pageContent);
        Assert.Contains("Managed users", pageContent);
        Assert.Contains("Active user", pageContent);
        Assert.Contains("BuildBookRoles.All", pageContent);
        Assert.Contains("Edit user", pageContent);
        Assert.Contains("Save user", pageContent);
        Assert.Contains("Cancel", pageContent);
        Assert.Contains("Status", pageContent);
        Assert.Contains("Roles", pageContent);
        Assert.Contains("Spreadsheet Import", pageContent);
        Assert.Contains("/imports/upload", pageContent);
        Assert.Contains("Open Upload Screen", pageContent);
        Assert.Contains("/imports", pageContent);
        Assert.Contains("View Import History", pageContent);
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
            BuildBookPolicies.ImportSpreadsheet => nameof(BuildBookPolicies.ImportSpreadsheet),
            BuildBookPolicies.ExportNonSensitiveData => nameof(BuildBookPolicies.ExportNonSensitiveData),
            BuildBookPolicies.ManageUsers => nameof(BuildBookPolicies.ManageUsers),
            _ => throw new ArgumentOutOfRangeException(nameof(policy), policy, "No constant-name assertion is configured for this policy.")
        };
    }
}
