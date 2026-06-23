using BuildBook.Infrastructure;
using BuildBook.Infrastructure.Persistence.SeedData;
using BuildBook.Web.Authorization;
using BuildBook.Web.Configuration;
using BuildBook.Web.Components;
using Microsoft.AspNetCore.Authentication.Negotiate;

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
builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme)
    .AddNegotiate();
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
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .RequireAuthorization();

app.Run();

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
