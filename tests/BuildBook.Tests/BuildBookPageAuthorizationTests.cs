using BuildBook.Application.Security;

namespace BuildBook.Tests;

public class BuildBookPageAuthorizationTests
{
    [Theory]
    [InlineData("Home.razor", BuildBookPolicies.ViewBuildRecords)]
    [InlineData("BuildRegister.razor", BuildBookPolicies.ViewBuildRecords)]
    [InlineData("BuildRecordDetail.razor", BuildBookPolicies.ViewBuildRecords)]
    [InlineData("Orders.razor", BuildBookOrderPolicies.ViewOrders)]
    [InlineData("OrderImport.razor", BuildBookOrderPolicies.ImportOrders)]
    [InlineData("Rmas.razor", BuildBookRmaPolicies.ViewRmas)]
    [InlineData("RmaBoard.razor", BuildBookRmaPolicies.ViewRmas)]
    [InlineData("RmaDetail.razor", BuildBookRmaPolicies.ViewRmas)]
    [InlineData("RmaReports.razor", BuildBookRmaPolicies.ExportRmaReports)]
    [InlineData("CreateRma.razor", BuildBookRmaPolicies.CreateRmas)]
    [InlineData("CreateBuildRecord.razor", BuildBookPolicies.EditBuildRecords)]
    [InlineData("ImportSpreadsheet.razor", BuildBookPolicies.ImportSpreadsheet)]
    [InlineData("ImportHistory.razor", BuildBookPolicies.ImportSpreadsheet)]
    [InlineData("Customers.razor", BuildBookPolicies.ViewCustomers)]
    [InlineData("CustomerDetail.razor", BuildBookPolicies.ViewCustomers)]
    [InlineData("CreateCustomer.razor", BuildBookPolicies.AddCustomers)]
    [InlineData("EditCustomer.razor", BuildBookPolicies.EditCustomers)]
    [InlineData("CustomerReports.razor", BuildBookPolicies.ExportCustomers)]
    [InlineData("Reports.razor", BuildBookPolicies.ExportNonSensitiveData)]
    [InlineData("Admin.razor", BuildBookPolicies.ManageUsers)]
    [InlineData("SupportContractLevels.razor", BuildBookPolicies.ManageSupportContractLevels)]
    [InlineData("SystemSettings.razor", BuildBookPolicies.ManageSystemSettings)]
    public void PagesDeclareExpectedAuthorizationPolicy(string pageFileName, string expectedPolicy)
    {
        var pageContent = File.ReadAllText(GetPagePath(pageFileName));

        Assert.Contains($"@attribute [Authorize(Policy = {GetPolicyReference(expectedPolicy)})]", pageContent);
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
        Assert.Contains($"Policy=\"@BuildBookOrderPolicies.{nameof(BuildBookOrderPolicies.ViewOrders)}\"", layoutContent);
        Assert.Contains($"Policy=\"@BuildBookPolicies.{nameof(BuildBookPolicies.ViewCustomers)}\"", layoutContent);
        Assert.Contains($"Policy=\"@BuildBookRmaPolicies.{nameof(BuildBookRmaPolicies.ViewRmas)}\"", layoutContent);
        Assert.Contains($"Policy=\"@BuildBookPolicies.{nameof(BuildBookPolicies.ManageUsers)}\"", layoutContent);
        Assert.Contains("Orders", layoutContent);
        Assert.Contains("RMAs", layoutContent);
        Assert.Contains("Customers", layoutContent);
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
    public void RmaRegisterPageDefinesExpectedRouteFiltersAndTable()
    {
        var pageContent = File.ReadAllText(GetPagePath("Rmas.razor"));

        Assert.Contains("@page \"/rmas\"", pageContent);
        Assert.Contains("@rendermode InteractiveServer", pageContent);
        Assert.Contains("IRmaRecordService", pageContent);
        Assert.Contains("BuildBookRmaPolicies.ViewRmas", pageContent);
        Assert.Contains("BuildBookRmaPolicies.CreateRmas", pageContent);
        Assert.Contains("RMA Register", pageContent);
        Assert.Contains("Reports", pageContent);
        Assert.Contains("/rmas/reports", pageContent);
        Assert.Contains("Board view", pageContent);
        Assert.Contains("/rmas/board", pageContent);
        Assert.Contains("RMA dashboard summary", pageContent);
        Assert.Contains("Open RMAs", pageContent);
        Assert.Contains("Waiting for customer", pageContent);
        Assert.Contains("Waiting for parts", pageContent);
        Assert.Contains("Ready to ship", pageContent);
        Assert.Contains("Shipped not closed", pageContent);
        Assert.Contains("Show RMAs", pageContent);
        Assert.Contains("DashboardReportLink(RmaReportScope.OperationalOpen)", pageContent);
        Assert.Contains("DashboardReportLink(RmaReportScope.OperationalOverdue)", pageContent);
        Assert.Contains("DashboardReportLink(RmaReportScope.OperationalWaitingForCustomer)", pageContent);
        Assert.Contains("DashboardReportLink(RmaReportScope.OperationalWaitingForParts)", pageContent);
        Assert.Contains("DashboardReportLink(RmaReportScope.OperationalReadyToShip)", pageContent);
        Assert.Contains("DashboardReportLink(RmaReportScope.OperationalShippedNotClosed)", pageContent);
        Assert.Contains("FormName=\"rma-register-filters\"", pageContent);
        Assert.Contains("Apply filters", pageContent);
        Assert.Contains("Clear filters", pageContent);
        Assert.Contains("SortByAsync(RmaRegisterSortColumn.RmaNumber)", pageContent);
        Assert.Contains("SortByAsync(RmaRegisterSortColumn.LastUpdated)", pageContent);
        Assert.Contains("SortIndicator(RmaRegisterSortColumn.RmaNumber)", pageContent);
        Assert.Contains("SortIndicator(RmaRegisterSortColumn.LastUpdated)", pageContent);
        Assert.Contains("Search", pageContent);
        Assert.Contains("Customer", pageContent);
        Assert.Contains("Product", pageContent);
        Assert.Contains("Serial number", pageContent);
        Assert.Contains("Assigned to", pageContent);
        Assert.Contains("Priority", pageContent);
        Assert.Contains("Due date", pageContent);
        Assert.Contains("Build Record link", pageContent);
        Assert.Contains("Loading RMA records", pageContent);
        Assert.Contains("No RMA records found", pageContent);
        Assert.Contains("RMA number", pageContent);
        Assert.Contains("Fault summary", pageContent);
        Assert.Contains("Build Record", pageContent);
        Assert.Contains("/rmas/{rmaRecord.Id}", pageContent);
        Assert.DoesNotContain("Password", pageContent);
        Assert.DoesNotContain("BitLocker", pageContent);
    }

    [Fact]
    public void OrdersPageDefinesExpectedRoutePlaceholderAndWorkflowStatuses()
    {
        var pageContent = File.ReadAllText(GetPagePath("Orders.razor"));

        Assert.Contains("@page \"/orders\"", pageContent);
        Assert.Contains("@rendermode InteractiveServer", pageContent);
        Assert.Contains("BuildBookOrderPolicies.ViewOrders", pageContent);
        Assert.Contains("Orders", pageContent);
        Assert.Contains("Foundation ready", pageContent);
        Assert.Contains("BuildBookOrderStatuses.DefaultWorkflow", pageContent);
        Assert.Contains("Import Planner Export", pageContent);
        Assert.Contains("/orders/import", pageContent);
        Assert.Contains("Default workflow statuses", pageContent);
        Assert.Contains("@foreach (var status in BuildBookOrderStatuses.DefaultWorkflow)", pageContent);
        Assert.DoesNotContain("Password", pageContent);
        Assert.DoesNotContain("BitLocker", pageContent);
    }

    [Fact]
    public void OrderImportPageDefinesExpectedRouteAndImportWorkflow()
    {
        var pageContent = File.ReadAllText(GetPagePath("OrderImport.razor"));

        Assert.Contains("@page \"/orders/import\"", pageContent);
        Assert.Contains("@rendermode InteractiveServer", pageContent);
        Assert.Contains("BuildBookOrderPolicies.ImportOrders", pageContent);
        Assert.Contains("IOrderPlannerImportService", pageContent);
        Assert.Contains("AuthenticationStateProvider", pageContent);
        Assert.Contains("ILogger<OrderImport>", pageContent);
        Assert.Contains("Upload Planner Export", pageContent);
        Assert.Contains("InputFile", pageContent);
        Assert.Contains("BuildReviewAsync", pageContent);
        Assert.Contains("Import Review", pageContent);
        Assert.Contains("Import Validation", pageContent);
        Assert.Contains("Import Summary", pageContent);
        Assert.Contains("Preferred worksheet", pageContent);
        Assert.Contains("Plan name", pageContent);
        Assert.Contains("Task rows read", pageContent);
        Assert.Contains("Rows checked", pageContent);
        Assert.Contains("Task ID", pageContent);
        Assert.Contains("Run Import", pageContent);
        Assert.Contains("BuildImportAsync", pageContent);
        Assert.Contains("HandleUnexpectedException", pageContent);
        Assert.Contains("The spreadsheet must be 25 MB or smaller.", pageContent);
        Assert.Contains("Choose an Excel workbook or CSV file.", pageContent);
        Assert.Contains("Planner task rows have been converted into BuildBook Orders", pageContent);
    }

    [Fact]
    public void CreateRmaPageDefinesExpectedRouteFormAndBuildRecordMatching()
    {
        var pageContent = File.ReadAllText(GetPagePath("CreateRma.razor"));

        Assert.Contains("@page \"/rmas/new\"", pageContent);
        Assert.Contains("@rendermode InteractiveServer", pageContent);
        Assert.Contains("BuildBookRmaPolicies.CreateRmas", pageContent);
        Assert.Contains("IRmaRecordService", pageContent);
        Assert.Contains("ICustomerOptionsReader", pageContent);
        Assert.Contains("AuthenticationStateProvider", pageContent);
        Assert.Contains("NavigationManager", pageContent);
        Assert.Contains("FormName=\"create-rma-record\"", pageContent);
        Assert.Contains("Customer", pageContent);
        Assert.Contains("InputSelect id=\"rma-customer-id\"", pageContent);
        Assert.Contains("Select customer", pageContent);
        Assert.Contains("Product name", pageContent);
        Assert.Contains("Product code", pageContent);
        Assert.Contains("Serial number", pageContent);
        Assert.Contains("Fault summary", pageContent);
        Assert.Contains("Fault description", pageContent);
        Assert.Contains("ISystemSettingsService", pageContent);
        Assert.Contains("SupportTicketLabel", pageContent);
        Assert.Contains("SupportTicketSettingsValidator.DefaultSupportTicketLabel", pageContent);
        Assert.DoesNotContain("Support ticket URL", pageContent);
        Assert.Contains("Contact name", pageContent);
        Assert.Contains("Original order number", pageContent);
        Assert.Contains("Original invoice number", pageContent);
        Assert.Contains("Suggested Build Records", pageContent);
        Assert.Contains("Repeat Return Check", pageContent);
        Assert.Contains("GetRepeatReturnSummaryAsync", pageContent);
        Assert.Contains("SupplyParameterFromQuery", pageContent);
        Assert.Contains("GetCreatePrefillAsync", pageContent);
        Assert.Contains("Pre-filled from Build Record", pageContent);
        Assert.Contains("RefreshSuggestionsAsync", pageContent);
        Assert.Contains("HandleCustomerSelectionChangedAsync", pageContent);
        Assert.Contains("SelectBuildRecordMatch", pageContent);
        Assert.Contains("Continue unlinked", pageContent);
        Assert.Contains("RmaRecordService.CreateAsync", pageContent);
        Assert.Contains("RmaRecordService.SuggestBuildRecordMatchesAsync", pageContent);
        Assert.Contains("NavigationManager.NavigateTo($\"/rmas/{result.RmaRecordId}\")", pageContent);
    }

    [Fact]
    public void RmaDetailPageDefinesExpectedRouteSummaryEditingAndBuildRecordLinking()
    {
        var pageContent = File.ReadAllText(GetPagePath("RmaDetail.razor"));

        Assert.Contains("@page \"/rmas/{RmaRecordId:int}\"", pageContent);
        Assert.Contains("@rendermode InteractiveServer", pageContent);
        Assert.Contains("BuildBookRmaPolicies.ViewRmas", pageContent);
        Assert.Contains("BuildBookRmaPolicies.EditRmas", pageContent);
        Assert.Contains("BuildBookRmaPolicies.ChangeRmaStatus", pageContent);
        Assert.Contains("IRmaRecordService", pageContent);
        Assert.Contains("ICustomerOptionsReader", pageContent);
        Assert.Contains("IRmaStatusTransitionService", pageContent);
        Assert.Contains("AuthenticationStateProvider", pageContent);
        Assert.Contains("Summary", pageContent);
        Assert.Contains("RecordSectionLink(\"rma-summary\")", pageContent);
        Assert.Contains("RecordSectionLink(\"rma-build-record-link\")", pageContent);
        Assert.Contains("Workflow &amp; Assignment", pageContent);
        Assert.Contains("Status Workflow", pageContent);
        Assert.Contains("Status History", pageContent);
        Assert.Contains("Intake &amp; Customer", pageContent);
        Assert.Contains("Fault Details", pageContent);
        Assert.Contains("Warranty &amp; Commercial", pageContent);
        Assert.Contains("Diagnosis &amp; Repair", pageContent);
        Assert.Contains("Parts Replaced", pageContent);
        Assert.Contains("Repair Checklist", pageContent);
        Assert.Contains("Testing and QA", pageContent);
        Assert.Contains("Build Record link", pageContent);
        Assert.Contains("Repeat Return History", pageContent);
        Assert.Contains("RMA Metrics", pageContent);
        Assert.Contains("IRmaReportReader", pageContent);
        Assert.Contains("GetMetricsAsync", pageContent);
        Assert.Contains("Days open", pageContent);
        Assert.Contains("Days in current status", pageContent);
        Assert.Contains("Days on hold", pageContent);
        Assert.Contains("GetRepeatReturnSummaryAsync", pageContent);
        Assert.Contains("FormName=\"edit-rma-intake\"", pageContent);
        Assert.Contains("FormName=\"edit-rma-fault-details\"", pageContent);
        Assert.Contains("FormName=\"edit-rma-repair-details\"", pageContent);
        Assert.Contains("FormName=\"edit-rma-workflow\"", pageContent);
        Assert.Contains("FormName=\"edit-rma-testing-qa\"", pageContent);
        Assert.Contains("FormName=\"edit-rma-part\"", pageContent);
        Assert.Contains("FormName=\"add-rma-checklist-item\"", pageContent);
        Assert.Contains("FormName=\"change-rma-status\"", pageContent);
        Assert.Contains("Target completion date", pageContent);
        Assert.Contains("On hold reason", pageContent);
        Assert.Contains("Closure notes", pageContent);
        Assert.Contains("Ready To Ship warnings were found.", pageContent);
        Assert.Contains("Continue with the status change anyway", pageContent);
        Assert.Contains("AllowedNextStatuses", pageContent);
        Assert.Contains("GetChecklistAsync", pageContent);
        Assert.Contains("GetPartsAsync", pageContent);
        Assert.Contains("SaveFaultDetailsAsync", pageContent);
        Assert.Contains("SaveRepairDetailsAsync", pageContent);
        Assert.Contains("SaveTestingQaAsync", pageContent);
        Assert.Contains("ToggleChecklistItemAsync", pageContent);
        Assert.Contains("SaveWorkflowAsync", pageContent);
        Assert.Contains("SaveStatusAsync", pageContent);
        Assert.Contains("LoadRmaAsync", pageContent);
        Assert.Contains("GetStatusHistoryAsync", pageContent);
        Assert.Contains("Customer reference", pageContent);
        Assert.Contains("FormName=\"edit-rma-warranty-commercial\"", pageContent);
        Assert.Contains("Customer approval required", pageContent);
        Assert.Contains("Estimated repair cost", pageContent);
        Assert.Contains("InputSelect id=\"edit-rma-customer\"", pageContent);
        Assert.Contains("Select customer", pageContent);
        Assert.Contains("selectedCustomerId", pageContent);
        Assert.Contains("Original order date", pageContent);
        Assert.Contains("Customer address", pageContent);
        Assert.Contains("Initial fault description", pageContent);
        Assert.Contains("Fault description", pageContent);
        Assert.Contains("LoadBuildRecordSuggestionsAsync", pageContent);
        Assert.Contains("SaveIntakeAsync", pageContent);
        Assert.Contains("LinkBuildRecordAsync", pageContent);
        Assert.Contains("UnlinkBuildRecordAsync", pageContent);
        Assert.Contains("/build-records/{rmaRecord.BuildRecordId}", pageContent);
        Assert.DoesNotContain("Password", pageContent);
        Assert.DoesNotContain("BitLocker", pageContent);
    }

    [Fact]
    public void RmaBoardPageDefinesExpectedRouteGroupingAndWarnings()
    {
        var pageContent = File.ReadAllText(GetPagePath("RmaBoard.razor"));

        Assert.Contains("@page \"/rmas/board\"", pageContent);
        Assert.Contains("@rendermode InteractiveServer", pageContent);
        Assert.Contains("BuildBookRmaPolicies.ViewRmas", pageContent);
        Assert.Contains("IRmaRecordService", pageContent);
        Assert.Contains("RMA Board", pageContent);
        Assert.Contains("GetBoardAsync", pageContent);
        Assert.Contains("BoardStatuses", pageContent);
        Assert.Contains("Checklist", pageContent);
        Assert.Contains("previous RMA", pageContent);
        Assert.Contains("Overdue", pageContent);
        Assert.Contains("board-warning-list", pageContent);
        Assert.Contains("/rmas/{card.Id}", pageContent);
    }

    [Fact]
    public void RmaReportsPageDefinesExpectedReportsMetricsAndExports()
    {
        var pageContent = File.ReadAllText(GetPagePath("RmaReports.razor"));

        Assert.Contains("@page \"/rmas/reports\"", pageContent);
        Assert.Contains("@rendermode InteractiveServer", pageContent);
        Assert.Contains("BuildBookRmaPolicies.ExportRmaReports", pageContent);
        Assert.Contains("IRmaReportReader", pageContent);
        Assert.Contains("RMA Reports", pageContent);
        Assert.Contains("Operational reports", pageContent);
        Assert.Contains("Customer and product reports", pageContent);
        Assert.Contains("Repeat-return serial numbers", pageContent);
        Assert.Contains("Customer devices with multiple RMAs", pageContent);
        Assert.Contains("Fault and root cause reports", pageContent);
        Assert.Contains("Product and fault combinations", pageContent);
        Assert.Contains("Warranty and commercial reports", pageContent);
        Assert.Contains("RMA metrics", pageContent);
        Assert.Contains("Average days open", pageContent);
        Assert.Contains("ExportUrl(\"csv\")", pageContent);
        Assert.Contains("ExportUrl(\"xlsx\")", pageContent);
        Assert.Contains("ReportScopeQuery", pageContent);
        Assert.Contains("ReportValueQuery", pageContent);
        Assert.Contains("ApplySelectedReport", pageContent);
        Assert.Contains("BuildSummaries", pageContent);
        Assert.Contains("LoadReportsAsync", pageContent);
        Assert.Contains("scope=", pageContent);
        Assert.Contains("#selected-rma-report-heading", pageContent);
        Assert.DoesNotContain("Password", pageContent);
        Assert.DoesNotContain("BitLocker", pageContent);
    }

    [Fact]
    public void DashboardSummaryCardDefinesSharedCardLayout()
    {
        var componentPath = Path.Combine(
            GetRepositoryRoot(),
            "src",
            "BuildBook.Web",
            "Components",
            "DashboardSummaryCard.razor");
        var componentContent = File.ReadAllText(componentPath);

        Assert.Contains("dashboard-card dashboard-card-stack", componentContent);
        Assert.Contains("dashboard-card-label", componentContent);
        Assert.Contains("dashboard-card-value", componentContent);
        Assert.Contains("dashboard-card-link", componentContent);
        Assert.Contains("LinkHref", componentContent);
        Assert.Contains("LinkLabel", componentContent);
        Assert.Contains("ValueCaption", componentContent);
        Assert.Contains("Body", componentContent);
        Assert.Contains("EditorRequired", componentContent);
    }

    [Fact]
    public void BuildRegisterPageDefinesExpectedTable()
    {
        var pageContent = File.ReadAllText(GetPagePath("BuildRegister.razor"));

        Assert.Contains("IBuildRegisterReader", pageContent);
        Assert.Contains("Reports", pageContent);
        Assert.Contains("href=\"/reports\"", pageContent);
        Assert.Contains("FormName=\"build-register-filters\"", pageContent);
        Assert.Contains("Apply filters", pageContent);
        Assert.Contains("Clear filters", pageContent);
        Assert.Contains("canClearFilters", pageContent);
        Assert.Contains("BuildBookPolicies.ExportNonSensitiveData", pageContent);
        Assert.Contains("Export CSV", pageContent);
        Assert.Contains("Export Excel", pageContent);
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
        Assert.Contains("Export the currently selected report", pageContent);
        Assert.Contains("Export CSV", pageContent);
        Assert.Contains("Export Excel", pageContent);
        Assert.Contains("CurrentCsvExportUrl", pageContent);
        Assert.Contains("CurrentExcelExportUrl", pageContent);
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
        Assert.Contains("BuildExportUrl", pageContent);
        Assert.Contains("/reports/build-register.{format}", pageContent);
        Assert.Contains("/reports/missing-data.{format}", pageContent);
        Assert.Contains("customerId=", pageContent);
        Assert.Contains("The version reports could not be loaded. Refresh the page and try again.", pageContent);
        Assert.Contains("BuildVersionReportLink(\"radSightVersion\"", pageContent);
        Assert.Contains("BuildVersionReportLink(\"windowsVersion\"", pageContent);
        Assert.Contains("Open matching records", pageContent);
        Assert.Contains("No RadSight versions have been recorded yet.", pageContent);
        Assert.Contains("No Windows versions have been recorded yet.", pageContent);
        Assert.DoesNotContain("Open Build Register", pageContent);
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
        Assert.Contains("IRmaRecordService", pageContent);
        Assert.Contains("IBuildBookPermissionService", pageContent);
        Assert.Contains("IRecentlyViewedBuildRecordTracker", pageContent);
        Assert.Contains("FormName=\"edit-product-details\"", pageContent);
        Assert.Contains("FormName=\"edit-build-details\"", pageContent);
        Assert.Contains("FormName=\"edit-customer-shipping\"", pageContent);
        Assert.Contains("FormName=\"edit-hardware-details\"", pageContent);
        Assert.Contains("FormName=\"edit-software-firmware\"", pageContent);
        Assert.Contains("FormName=\"edit-network-details\"", pageContent);
        Assert.Contains("FormName=\"edit-notes\"", pageContent);
        Assert.Contains("RecordSectionLink(\"build-record-summary\")", pageContent);
        Assert.Contains("RecordSectionLink(\"history\")", pageContent);
        Assert.Contains("Product Details", pageContent);
        Assert.Contains("Build Details", pageContent);
        Assert.Contains("Customer &amp; Shipping", pageContent);
        Assert.Contains("Hardware", pageContent);
        Assert.Contains("Software &amp; Firmware", pageContent);
        Assert.Contains("Network", pageContent);
        Assert.Contains("Credentials &amp; Recovery", pageContent);
        Assert.Contains("Notes", pageContent);
        Assert.Contains("Linked RMAs", pageContent);
        Assert.Contains("Create RMA", pageContent);
        Assert.Contains("GetBuildRecordHistoryAsync", pageContent);
        Assert.Contains("/rmas/new?buildRecordId={BuildRecordId}", pageContent);
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
        Assert.Contains("Bootstrap administrators", pageContent);
        Assert.Contains("Add Windows user", pageContent);
        Assert.Contains("Windows username", pageContent);
        Assert.Contains("Display name", pageContent);
        Assert.Contains("Email address", pageContent);
        Assert.Contains("Managed users", pageContent);
        Assert.Contains("Bootstrap administrator", pageContent);
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
        Assert.Contains("Customer Support Contracts", pageContent);
        Assert.Contains("/admin/support-contract-levels", pageContent);
        Assert.Contains("Open Support Contract Levels", pageContent);
        Assert.Contains("System Settings", pageContent);
        Assert.Contains("/admin/system-settings", pageContent);
        Assert.Contains("Open System Settings", pageContent);
    }

    [Fact]
    public void CustomersPageDefinesListFiltersAndActions()
    {
        var pageContent = File.ReadAllText(GetPagePath("Customers.razor"));

        Assert.Contains("@page \"/customers\"", pageContent);
        Assert.Contains("@rendermode InteractiveServer", pageContent);
        Assert.Contains("BuildBookPolicies.ViewCustomers", pageContent);
        Assert.Contains("Customer &amp; Support Contracts", pageContent);
        Assert.Contains("ICustomerService", pageContent);
        Assert.Contains("ISupportContractLevelService", pageContent);
        Assert.Contains("FormName=\"customers-filters\"", pageContent);
        Assert.Contains("Support contract levels", pageContent);
        Assert.Contains("/customers/reports", pageContent);
        Assert.Contains("Reports", pageContent);
        Assert.Contains("/customers/new", pageContent);
        Assert.Contains("Customer name", pageContent);
        Assert.Contains("Support contract level", pageContent);
        Assert.Contains("Support contract status", pageContent);
        Assert.Contains("Apply filters", pageContent);
        Assert.Contains("Clear filters", pageContent);
    }

    [Fact]
    public void CustomerDetailPageDefinesExpectedSectionsAndLinkedTables()
    {
        var pageContent = File.ReadAllText(GetPagePath("CustomerDetail.razor"));

        Assert.Contains("@page \"/customers/{CustomerId:int}\"", pageContent);
        Assert.Contains("ICustomerService", pageContent);
        Assert.Contains("Summary", pageContent);
        Assert.Contains("Address", pageContent);
        Assert.Contains("Contacts", pageContent);
        Assert.Contains("Support Contract", pageContent);
        Assert.Contains("Linked Build Records", pageContent);
        Assert.Contains("Linked RMAs", pageContent);
        Assert.Contains("History", pageContent);
        Assert.Contains("/build-records/{buildRecord.Id}", pageContent);
        Assert.Contains("/rmas/{rmaRecord.Id}", pageContent);
        Assert.Contains("/customers/reports", pageContent);
        Assert.Contains("/customers/{customer.Id}/edit", pageContent);
    }

    [Fact]
    public void CustomerReportsPageDefinesExpectedReportsAndExports()
    {
        var pageContent = File.ReadAllText(GetPagePath("CustomerReports.razor"));

        Assert.Contains("@page \"/customers/reports\"", pageContent);
        Assert.Contains("BuildBookPolicies.ExportCustomers", pageContent);
        Assert.Contains("ICustomerReportReader", pageContent);
        Assert.Contains("Customer Reports", pageContent);
        Assert.Contains("Export CSV", pageContent);
        Assert.Contains("Export Excel", pageContent);
        Assert.Contains("Customers by contract level", pageContent);
        Assert.Contains("Expiring in 30 days", pageContent);
        Assert.Contains("Open RMAs by contract level", pageContent);
        Assert.Contains("Overdue RMAs by contract level", pageContent);
        Assert.Contains("RMAs with no Support Ticket No.", pageContent);
        Assert.Contains("Priority mismatch", pageContent);
        Assert.Contains("Support Ticket No.", pageContent);
        Assert.Contains("CurrentCsvExportUrl", pageContent);
        Assert.Contains("CurrentExcelExportUrl", pageContent);
        Assert.Contains("ReportLink(CustomerReportScope.PriorityMismatch)", pageContent);
        Assert.Contains("ReportLink(CustomerReportScope.MissingSupportTicketNumber)", pageContent);
        Assert.DoesNotContain("Password", pageContent);
        Assert.DoesNotContain("BitLocker", pageContent);
    }

    [Fact]
    public void CreateCustomerPageDefinesExpectedRouteAndForm()
    {
        var pageContent = File.ReadAllText(GetPagePath("CreateCustomer.razor"));

        Assert.Contains("@page \"/customers/new\"", pageContent);
        Assert.Contains("ICustomerService", pageContent);
        Assert.Contains("ISupportContractLevelService", pageContent);
        Assert.Contains("AuthenticationStateProvider", pageContent);
        Assert.Contains("NavigationManager", pageContent);
        Assert.Contains("FormName=\"create-customer\"", pageContent);
        Assert.Contains("Customer name", pageContent);
        Assert.Contains("Support contract level", pageContent);
        Assert.Contains("Support contract status", pageContent);
        Assert.Contains("Support notes", pageContent);
        Assert.Contains("Create customer", pageContent);
    }

    [Fact]
    public void EditCustomerPageDefinesExpectedRouteAndForm()
    {
        var pageContent = File.ReadAllText(GetPagePath("EditCustomer.razor"));

        Assert.Contains("@page \"/customers/{CustomerId:int}/edit\"", pageContent);
        Assert.Contains("ICustomerService", pageContent);
        Assert.Contains("ISupportContractLevelService", pageContent);
        Assert.Contains("AuthenticationStateProvider", pageContent);
        Assert.Contains("NavigationManager", pageContent);
        Assert.Contains("FormName=\"edit-customer\"", pageContent);
        Assert.Contains("Save customer", pageContent);
        Assert.Contains("/customers/{CustomerId}", pageContent);
    }

    [Fact]
    public void SupportContractLevelsPageDefinesExpectedRouteAndManagementUi()
    {
        var pageContent = File.ReadAllText(GetPagePath("SupportContractLevels.razor"));

        Assert.Contains("@page \"/admin/support-contract-levels\"", pageContent);
        Assert.Contains("BuildBookPolicies.ManageSupportContractLevels", pageContent);
        Assert.Contains("ISupportContractLevelService", pageContent);
        Assert.Contains("AuthenticationStateProvider", pageContent);
        Assert.Contains("Add support contract level", pageContent);
        Assert.Contains("Current support contract levels", pageContent);
        Assert.Contains("showAddLevelForm", pageContent);
        Assert.Contains("ToggleAddLevelForm", pageContent);
        Assert.Contains("HideAddLevelForm", pageContent);
        Assert.Contains("FormName=\"create-support-contract-level\"", pageContent);
        Assert.Contains("edit-support-contract-level-", pageContent);
        Assert.Contains("Default RMA priority", pageContent);
        Assert.Contains("Display order", pageContent);
        Assert.Contains("/admin", pageContent);
    }

    [Fact]
    public void SystemSettingsPageDefinesExpectedRouteAndSupportTicketSettingsUi()
    {
        var pageContent = File.ReadAllText(GetPagePath("SystemSettings.razor"));

        Assert.Contains("@page \"/admin/system-settings\"", pageContent);
        Assert.Contains("BuildBookPolicies.ManageSystemSettings", pageContent);
        Assert.Contains("ISystemSettingsService", pageContent);
        Assert.Contains("AuthenticationStateProvider", pageContent);
        Assert.Contains("Support Site URL Template", pageContent);
        Assert.Contains("Support ticket label", pageContent);
        Assert.Contains("FormName=\"edit-system-settings\"", pageContent);
        Assert.Contains("Save settings", pageContent);
        Assert.Contains("/admin", pageContent);
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
            BuildBookPolicies.ViewCustomers => nameof(BuildBookPolicies.ViewCustomers),
            BuildBookPolicies.AddCustomers => nameof(BuildBookPolicies.AddCustomers),
            BuildBookPolicies.EditCustomers => nameof(BuildBookPolicies.EditCustomers),
            BuildBookPolicies.ExportCustomers => nameof(BuildBookPolicies.ExportCustomers),
            BuildBookPolicies.ImportSpreadsheet => nameof(BuildBookPolicies.ImportSpreadsheet),
            BuildBookPolicies.ExportNonSensitiveData => nameof(BuildBookPolicies.ExportNonSensitiveData),
            BuildBookPolicies.ManageUsers => nameof(BuildBookPolicies.ManageUsers),
            BuildBookPolicies.ManageSupportContractLevels => nameof(BuildBookPolicies.ManageSupportContractLevels),
            BuildBookPolicies.ManageSystemSettings => nameof(BuildBookPolicies.ManageSystemSettings),
            BuildBookOrderPolicies.ViewOrders => nameof(BuildBookOrderPolicies.ViewOrders),
            BuildBookRmaPolicies.ViewRmas => nameof(BuildBookRmaPolicies.ViewRmas),
            BuildBookRmaPolicies.CreateRmas => nameof(BuildBookRmaPolicies.CreateRmas),
            BuildBookRmaPolicies.ExportRmaReports => nameof(BuildBookRmaPolicies.ExportRmaReports),
            _ => throw new ArgumentOutOfRangeException(nameof(policy), policy, "No constant-name assertion is configured for this policy.")
        };
    }

    private static string GetPolicyReference(string policy)
    {
        return policy switch
        {
            BuildBookOrderPolicies.ViewOrders => $"BuildBookOrderPolicies.{nameof(BuildBookOrderPolicies.ViewOrders)}",
            BuildBookOrderPolicies.ImportOrders => $"BuildBookOrderPolicies.{nameof(BuildBookOrderPolicies.ImportOrders)}",
            BuildBookRmaPolicies.ViewRmas => $"BuildBookRmaPolicies.{nameof(BuildBookRmaPolicies.ViewRmas)}",
            BuildBookRmaPolicies.CreateRmas => $"BuildBookRmaPolicies.{nameof(BuildBookRmaPolicies.CreateRmas)}",
            BuildBookRmaPolicies.ExportRmaReports => $"BuildBookRmaPolicies.{nameof(BuildBookRmaPolicies.ExportRmaReports)}",
            _ => $"BuildBookPolicies.{GetPolicyConstantName(policy)}"
        };
    }
}
