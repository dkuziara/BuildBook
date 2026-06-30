using System.Text;
using BuildBook.Application.BuildRecords;
using BuildBook.Application.Rmas;
using BuildBook.Application.Security;
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

static string? ReadString(IQueryCollection query, string key)
{
    return query.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value)
        ? value.ToString()
        : null;
}
