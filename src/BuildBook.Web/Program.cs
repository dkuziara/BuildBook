using System.Text;
using BuildBook.Application.BuildRecords;
using BuildBook.Application.Customers;
using BuildBook.Application.Orders;
using BuildBook.Application.Rmas;
using BuildBook.Application.Security;
using BuildBook.Domain.Rmas;
using BuildBook.Infrastructure;
using BuildBook.Infrastructure.Persistence;
using BuildBook.Infrastructure.Persistence.SeedData;
using BuildBook.Web.Authorization;
using BuildBook.Web.Configuration;
using BuildBook.Web.Components;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging"));
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.AddEventSourceLogger();

builder.Services.AddOptions<BuildBookOptions>()
    .Bind(builder.Configuration.GetSection(BuildBookOptions.SectionName))
    .Validate(options => options.IsValid(), "BuildBook configuration is invalid.")
    .ValidateOnStart();

builder.Services.AddBuildBookInfrastructure(builder.Configuration);
builder.Services.AddBuildBookAuthentication(builder.Environment, builder.Configuration);
builder.Services.AddBuildBookAuthorization();
builder.Services.AddCascadingAuthenticationState();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents(options =>
    {
        options.DetailedErrors = builder.Configuration.GetValue<bool>("BuildBook:EnableDetailedErrors");
    });

var app = builder.Build();
var logger = app.Services.GetRequiredService<ILogger<Program>>();
var buildBookOptions = app.Services.GetRequiredService<Microsoft.Extensions.Options.IOptions<BuildBookOptions>>().Value;

logger.LogInformation(
    "{ApplicationName} starting in {EnvironmentName}. Detailed errors enabled: {DetailedErrorsEnabled}",
    buildBookOptions.ApplicationName,
    app.Environment.EnvironmentName,
    buildBookOptions.EnableDetailedErrors);

await InitializeDatabaseAsync(app, logger);

if (app.Environment.IsDevelopment() && buildBookOptions.SeedDevelopmentData)
{
    await SeedDevelopmentDataAsync(app, logger);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapGet(
        "/customers/{customerId:int}/contract-documents/{documentId:int}",
        async (int customerId, int documentId, ICustomerService customerService, CancellationToken cancellationToken) =>
        {
            var document = await customerService.GetContractDocumentContentAsync(customerId, documentId, cancellationToken);
            return document is null
                ? Results.NotFound()
                : Results.File(
                    document.Content,
                    document.ContentType,
                    document.FileName);
        })
    .RequireAuthorization(BuildBookPolicies.ViewCustomers);
app.MapGet(
        "/rmas/{rmaRecordId:int}/attachments/{attachmentId:int}",
        async (int rmaRecordId, int attachmentId, IRmaRecordService rmaRecordService, CancellationToken cancellationToken) =>
        {
            var attachment = await rmaRecordService.GetAttachmentContentAsync(rmaRecordId, attachmentId, cancellationToken);
            return attachment is null
                ? Results.NotFound()
                : Results.File(
                    attachment.Content,
                    attachment.ContentType,
                    attachment.FileName);
        })
    .RequireAuthorization(BuildBookRmaPolicies.ViewRmas);
app.MapGet(
        "/orders/export.csv",
        async (HttpRequest request, IOrderRegisterCsvExporter orderRegisterCsvExporter, CancellationToken cancellationToken) =>
        {
            var filter = CreateOrderRegisterFilter(request);
            var csv = await orderRegisterCsvExporter.ExportAsync(filter, cancellationToken);
            var fileName = $"order-register-{DateTimeOffset.UtcNow:yyyyMMdd-HHmmss}.csv";

            return Results.File(
                Encoding.UTF8.GetBytes(csv),
                "text/csv; charset=utf-8",
                fileName);
        })
    .RequireAuthorization(BuildBookOrderPolicies.ExportOrders);
app.MapGet(
        "/orders/export.xlsx",
        async (HttpRequest request, IOrderRegisterExcelExporter orderRegisterExcelExporter, CancellationToken cancellationToken) =>
        {
            var filter = CreateOrderRegisterFilter(request);
            var workbook = await orderRegisterExcelExporter.ExportAsync(filter, cancellationToken);
            var fileName = $"order-register-{DateTimeOffset.UtcNow:yyyyMMdd-HHmmss}.xlsx";

            return Results.File(
                workbook,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        })
    .RequireAuthorization(BuildBookOrderPolicies.ExportOrders);
app.MapGet(
        "/orders/reports/export.csv",
        async (HttpRequest request, IOrderReportCsvExporter orderReportCsvExporter, CancellationToken cancellationToken) =>
        {
            var filter = CreateOrderReportFilter(request);
            var csv = await orderReportCsvExporter.ExportAsync(filter, cancellationToken);
            var fileName = $"order-report-{DateTimeOffset.UtcNow:yyyyMMdd-HHmmss}.csv";

            return Results.File(
                Encoding.UTF8.GetBytes(csv),
                "text/csv; charset=utf-8",
                fileName);
        })
    .RequireAuthorization(BuildBookOrderPolicies.ExportOrders);
app.MapGet(
        "/orders/reports/export.xlsx",
        async (HttpRequest request, IOrderReportExcelExporter orderReportExcelExporter, CancellationToken cancellationToken) =>
        {
            var filter = CreateOrderReportFilter(request);
            var workbook = await orderReportExcelExporter.ExportAsync(filter, cancellationToken);
            var fileName = $"order-report-{DateTimeOffset.UtcNow:yyyyMMdd-HHmmss}.xlsx";

            return Results.File(
                workbook,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        })
    .RequireAuthorization(BuildBookOrderPolicies.ExportOrders);
app.MapGet(
        "/reports/build-register.csv",
        async (HttpRequest request, IBuildRegisterCsvExporter buildRegisterCsvExporter, CancellationToken cancellationToken) =>
        {
            var filter = CreateBuildRegisterFilter(request);
            var csv = await buildRegisterCsvExporter.ExportAsync(filter, cancellationToken);
            var fileName = $"build-register-{DateTimeOffset.UtcNow:yyyyMMdd-HHmmss}.csv";

            return Results.File(
                Encoding.UTF8.GetBytes(csv),
                "text/csv; charset=utf-8",
                fileName);
        })
    .RequireAuthorization(BuildBookPolicies.ExportNonSensitiveData);
app.MapGet(
        "/reports/missing-data.csv",
        async (HttpRequest request, IMissingDataReportCsvExporter missingDataReportCsvExporter, CancellationToken cancellationToken) =>
        {
            var reportType = ParseMissingDataReportType(ReadString(request.Query, "missingData"));
            if (reportType is null)
            {
                return Results.BadRequest();
            }

            var csv = await missingDataReportCsvExporter.ExportAsync(reportType.Value, cancellationToken);
            var fileName = $"build-report-{reportType.Value}-{DateTimeOffset.UtcNow:yyyyMMdd-HHmmss}.csv";

            return Results.File(
                Encoding.UTF8.GetBytes(csv),
                "text/csv; charset=utf-8",
                fileName);
        })
    .RequireAuthorization(BuildBookPolicies.ExportNonSensitiveData);
app.MapGet(
        "/reports/build-register.xlsx",
        async (HttpRequest request, IBuildRegisterExcelExporter buildRegisterExcelExporter, CancellationToken cancellationToken) =>
        {
            var filter = CreateBuildRegisterFilter(request);
            var workbook = await buildRegisterExcelExporter.ExportAsync(filter, cancellationToken);
            var fileName = $"build-register-{DateTimeOffset.UtcNow:yyyyMMdd-HHmmss}.xlsx";

            return Results.File(
                workbook,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        })
    .RequireAuthorization(BuildBookPolicies.ExportNonSensitiveData);
app.MapGet(
        "/reports/missing-data.xlsx",
        async (HttpRequest request, IMissingDataReportExcelExporter missingDataReportExcelExporter, CancellationToken cancellationToken) =>
        {
            var reportType = ParseMissingDataReportType(ReadString(request.Query, "missingData"));
            if (reportType is null)
            {
                return Results.BadRequest();
            }

            var workbook = await missingDataReportExcelExporter.ExportAsync(reportType.Value, cancellationToken);
            var fileName = $"build-report-{reportType.Value}-{DateTimeOffset.UtcNow:yyyyMMdd-HHmmss}.xlsx";

            return Results.File(
                workbook,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        })
    .RequireAuthorization(BuildBookPolicies.ExportNonSensitiveData);
app.MapGet(
        "/customers/export.csv",
        async (HttpRequest request, ICustomerListCsvExporter customerListCsvExporter, CancellationToken cancellationToken) =>
        {
            var filter = CreateCustomerListFilter(request);
            var csv = await customerListCsvExporter.ExportAsync(filter, cancellationToken);
            var fileName = $"customers-{DateTimeOffset.UtcNow:yyyyMMdd-HHmmss}.csv";

            return Results.File(
                Encoding.UTF8.GetBytes(csv),
                "text/csv; charset=utf-8",
                fileName);
        })
    .RequireAuthorization(BuildBookPolicies.ExportCustomers);
app.MapGet(
        "/customers/export.xlsx",
        async (HttpRequest request, ICustomerListExcelExporter customerListExcelExporter, CancellationToken cancellationToken) =>
        {
            var filter = CreateCustomerListFilter(request);
            var workbook = await customerListExcelExporter.ExportAsync(filter, cancellationToken);
            var fileName = $"customers-{DateTimeOffset.UtcNow:yyyyMMdd-HHmmss}.xlsx";

            return Results.File(
                workbook,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        })
    .RequireAuthorization(BuildBookPolicies.ExportCustomers);
app.MapGet(
        "/customers/reports/export.csv",
        async (HttpRequest request, ICustomerReportCsvExporter customerReportCsvExporter, CancellationToken cancellationToken) =>
        {
            var filter = CreateCustomerReportFilter(request);
            var csv = await customerReportCsvExporter.ExportAsync(filter, cancellationToken);
            var fileName = $"customer-report-{DateTimeOffset.UtcNow:yyyyMMdd-HHmmss}.csv";

            return Results.File(
                Encoding.UTF8.GetBytes(csv),
                "text/csv; charset=utf-8",
                fileName);
        })
    .RequireAuthorization(BuildBookPolicies.ExportCustomers);
app.MapGet(
        "/customers/reports/export.xlsx",
        async (HttpRequest request, ICustomerReportExcelExporter customerReportExcelExporter, CancellationToken cancellationToken) =>
        {
            var filter = CreateCustomerReportFilter(request);
            var workbook = await customerReportExcelExporter.ExportAsync(filter, cancellationToken);
            var fileName = $"customer-report-{DateTimeOffset.UtcNow:yyyyMMdd-HHmmss}.xlsx";

            return Results.File(
                workbook,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        })
    .RequireAuthorization(BuildBookPolicies.ExportCustomers);
app.MapGet(
        "/rmas/export.csv",
        async (HttpRequest request, IRmaRegisterCsvExporter rmaRegisterCsvExporter, CancellationToken cancellationToken) =>
        {
            var filter = CreateRmaRegisterFilter(request);
            var csv = await rmaRegisterCsvExporter.ExportAsync(filter, cancellationToken);
            var fileName = $"rma-register-{DateTimeOffset.UtcNow:yyyyMMdd-HHmmss}.csv";

            return Results.File(
                Encoding.UTF8.GetBytes(csv),
                "text/csv; charset=utf-8",
                fileName);
        })
    .RequireAuthorization(BuildBookRmaPolicies.ExportRmaReports);
app.MapGet(
        "/rmas/export.xlsx",
        async (HttpRequest request, IRmaRegisterExcelExporter rmaRegisterExcelExporter, CancellationToken cancellationToken) =>
        {
            var filter = CreateRmaRegisterFilter(request);
            var workbook = await rmaRegisterExcelExporter.ExportAsync(filter, cancellationToken);
            var fileName = $"rma-register-{DateTimeOffset.UtcNow:yyyyMMdd-HHmmss}.xlsx";

            return Results.File(
                workbook,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        })
    .RequireAuthorization(BuildBookRmaPolicies.ExportRmaReports);
app.MapGet(
        "/rmas/reports/export.csv",
        async (HttpRequest request, IRmaReportCsvExporter rmaReportCsvExporter, CancellationToken cancellationToken) =>
        {
            var filter = CreateRmaReportFilter(request);
            var csv = await rmaReportCsvExporter.ExportAsync(filter, cancellationToken);
            var fileName = $"rma-report-{DateTimeOffset.UtcNow:yyyyMMdd-HHmmss}.csv";

            return Results.File(
                Encoding.UTF8.GetBytes(csv),
                "text/csv; charset=utf-8",
                fileName);
        })
    .RequireAuthorization(BuildBookRmaPolicies.ExportRmaReports);
app.MapGet(
        "/rmas/reports/export.xlsx",
        async (HttpRequest request, IRmaReportExcelExporter rmaReportExcelExporter, CancellationToken cancellationToken) =>
        {
            var filter = CreateRmaReportFilter(request);
            var workbook = await rmaReportExcelExporter.ExportAsync(filter, cancellationToken);
            var fileName = $"rma-report-{DateTimeOffset.UtcNow:yyyyMMdd-HHmmss}.xlsx";

            return Results.File(
                workbook,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        })
    .RequireAuthorization(BuildBookRmaPolicies.ExportRmaReports);

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .RequireAuthorization();

app.Run();

static async Task InitializeDatabaseAsync(WebApplication app, ILogger logger)
{
    try
    {
        using var scope = app.Services.CreateScope();
        var databaseInitializer = scope.ServiceProvider.GetRequiredService<BuildBookDatabaseInitializer>();

        await databaseInitializer.InitializeAsync();
    }
    catch (Exception exception)
    {
        logger.LogError(
            exception,
            "BuildBook database initialization failed. The application will not start.");

        throw;
    }
}

static async Task SeedDevelopmentDataAsync(WebApplication app, ILogger logger)
{
    try
    {
        using var scope = app.Services.CreateScope();
        var seeder = scope.ServiceProvider.GetRequiredService<DevelopmentDataSeeder>();

        await seeder.SeedAsync();
    }
    catch (Exception exception)
    {
        logger.LogWarning(
            exception,
            "Development seed data could not be added. The application will continue to start.");
    }
}

static BuildRegisterFilter CreateBuildRegisterFilter(HttpRequest request)
{
    var query = request.Query;
    var filter = new BuildRegisterFilter();

    if (int.TryParse(ReadString(query, "customerId"), out var customerId))
    {
        filter.CustomerId = customerId;
    }

    filter.Customer = ReadString(query, "customer");
    filter.ProductCode = ReadString(query, "productCode");
    filter.RadSightVersion = ReadString(query, "radSightVersion");
    filter.WindowsVersion = ReadString(query, "windowsVersion");

    if (DateOnly.TryParse(ReadString(query, "dateShipped"), out var dateShipped))
    {
        filter.DateShipped = dateShipped;
    }

    if (Enum.TryParse<BuildRegisterSortColumn>(ReadString(query, "sortBy"), ignoreCase: true, out var sortBy))
    {
        filter.SortBy = sortBy;
    }

    if (bool.TryParse(ReadString(query, "sortDescending"), out var sortDescending))
    {
        filter.SortDescending = sortDescending;
    }

    return filter;
}

static RmaReportFilter CreateRmaReportFilter(HttpRequest request)
{
    var scopeValue = ReadString(request.Query, "scope");
    var value = ReadString(request.Query, "value");
    var filter = new RmaReportFilter
    {
        Value = value
    };

    if (Enum.TryParse<RmaReportScope>(scopeValue, ignoreCase: true, out var scope))
    {
        filter = new RmaReportFilter
        {
            Scope = scope,
            Value = value
        };
    }

    return filter;
}

static OrderReportFilter CreateOrderReportFilter(HttpRequest request)
{
    var scopeValue = ReadString(request.Query, "scope");
    var value = ReadString(request.Query, "value");
    var filter = new OrderReportFilter
    {
        Value = value
    };

    if (Enum.TryParse<OrderReportScope>(scopeValue, ignoreCase: true, out var scope))
    {
        filter = new OrderReportFilter
        {
            Scope = scope,
            Value = value
        };
    }

    return filter;
}

static OrderRegisterFilter CreateOrderRegisterFilter(HttpRequest request)
{
    var query = request.Query;
    var filter = new OrderRegisterFilter
    {
        Search = ReadString(query, "search"),
        Customer = ReadString(query, "customer"),
        AssignedTo = ReadString(query, "assignedTo"),
        Status = ReadString(query, "status")
    };

    if (Enum.TryParse<BuildBook.Domain.Orders.OrderPriority>(ReadString(query, "priority"), ignoreCase: true, out var priority))
    {
        filter.Priority = priority;
    }

    if (DateOnly.TryParse(ReadString(query, "dueDate"), out var dueDate))
    {
        filter.DueDate = dueDate;
    }

    if (bool.TryParse(ReadString(query, "isOverdue"), out var isOverdue))
    {
        filter.IsOverdue = isOverdue;
    }

    if (bool.TryParse(ReadString(query, "isCompleted"), out var isCompleted))
    {
        filter.IsCompleted = isCompleted;
    }

    if (bool.TryParse(ReadString(query, "hasLinkedBuildRecord"), out var hasLinkedBuildRecord))
    {
        filter.HasLinkedBuildRecord = hasLinkedBuildRecord;
    }

    if (Enum.TryParse<OrderRegisterSortColumn>(ReadString(query, "sortBy"), ignoreCase: true, out var sortBy))
    {
        filter.SortBy = sortBy;
    }

    if (bool.TryParse(ReadString(query, "sortDescending"), out var sortDescending))
    {
        filter.SortDescending = sortDescending;
    }

    return filter;
}

static RmaRegisterFilter CreateRmaRegisterFilter(HttpRequest request)
{
    var query = request.Query;
    var filter = new RmaRegisterFilter
    {
        Search = ReadString(query, "search"),
        Customer = ReadString(query, "customer"),
        Product = ReadString(query, "product"),
        SerialNumber = ReadString(query, "serialNumber"),
        AssignedTo = ReadString(query, "assignedTo")
    };

    if (Enum.TryParse<RmaStatus>(ReadString(query, "status"), ignoreCase: true, out var status))
    {
        filter.Status = status;
    }

    if (Enum.TryParse<RmaPriority>(ReadString(query, "priority"), ignoreCase: true, out var priority))
    {
        filter.Priority = priority;
    }

    if (DateOnly.TryParse(ReadString(query, "dueDate"), out var dueDate))
    {
        filter.DueDate = dueDate;
    }

    if (bool.TryParse(ReadString(query, "hasLinkedBuildRecord"), out var hasLinkedBuildRecord))
    {
        filter.HasLinkedBuildRecord = hasLinkedBuildRecord;
    }

    if (Enum.TryParse<RmaRegisterSortColumn>(ReadString(query, "sortBy"), ignoreCase: true, out var sortBy))
    {
        filter.SortBy = sortBy;
    }

    if (bool.TryParse(ReadString(query, "sortDescending"), out var sortDescending))
    {
        filter.SortDescending = sortDescending;
    }

    return filter;
}

static CustomerReportFilter CreateCustomerReportFilter(HttpRequest request)
{
    var scopeValue = ReadString(request.Query, "scope");
    var value = ReadString(request.Query, "value");
    var filter = new CustomerReportFilter
    {
        Value = value
    };

    if (Enum.TryParse<CustomerReportScope>(scopeValue, ignoreCase: true, out var scope))
    {
        filter = new CustomerReportFilter
        {
            Scope = scope,
            Value = value
        };
    }

    return filter;
}

static CustomerListFilter CreateCustomerListFilter(HttpRequest request)
{
    var query = request.Query;
    var filter = new CustomerListFilter
    {
        Search = ReadString(query, "search"),
        SupportContractStatus = ReadString(query, "supportContractStatus")
    };

    if (int.TryParse(ReadString(query, "supportContractLevelId"), out var supportContractLevelId))
    {
        filter.SupportContractLevelId = supportContractLevelId;
    }

    if (bool.TryParse(ReadString(query, "isActive"), out var isActive))
    {
        filter.IsActive = isActive;
    }

    if (Enum.TryParse<CustomerSortColumn>(ReadString(query, "sortBy"), ignoreCase: true, out var sortBy))
    {
        filter.SortBy = sortBy;
    }

    if (bool.TryParse(ReadString(query, "sortDescending"), out var sortDescending))
    {
        filter.SortDescending = sortDescending;
    }

    return filter;
}

static string? ReadString(IQueryCollection query, string key)
{
    return query.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value)
        ? value.ToString()
        : null;
}

static MissingDataReportType? ParseMissingDataReportType(string? value)
{
    return Enum.TryParse<MissingDataReportType>(value, ignoreCase: true, out var reportType)
        ? reportType
        : null;
}
