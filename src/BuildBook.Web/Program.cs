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

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
