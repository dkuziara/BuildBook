using BuildBook.Application.Security;
using BuildBook.Web.Authorization;
using BuildBook.Web.Configuration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BuildBook.Tests;

public class BuildBookAuthenticationTests
{
    [Fact]
    public async Task DevelopmentAuthenticationIsDefaultWhenEnabledInDevelopment()
    {
        using var provider = CreateProvider("Development", useDevelopmentAuthentication: true);
        var options = provider.GetRequiredService<IOptions<AuthenticationOptions>>().Value;

        Assert.Equal(BuildBookAuthenticationSchemes.Development, options.DefaultScheme);

        var schemeProvider = provider.GetRequiredService<IAuthenticationSchemeProvider>();
        var scheme = await schemeProvider.GetDefaultAuthenticateSchemeAsync();

        Assert.Equal(BuildBookAuthenticationSchemes.Development, scheme?.Name);
    }

    [Fact]
    public void NegotiateAuthenticationIsDefaultOutsideDevelopment()
    {
        using var provider = CreateProvider("Production", useDevelopmentAuthentication: true);
        var options = provider.GetRequiredService<IOptions<AuthenticationOptions>>().Value;

        Assert.Equal(NegotiateDefaults.AuthenticationScheme, options.DefaultScheme);
    }

    [Fact]
    public async Task DevelopmentAuthenticationCreatesSignedInUserWithConfiguredUserName()
    {
        using var provider = CreateProvider("Development", useDevelopmentAuthentication: true);
        var authenticationService = provider.GetRequiredService<IAuthenticationService>();
        var context = new DefaultHttpContext
        {
            RequestServices = provider
        };

        var result = await authenticationService.AuthenticateAsync(context, BuildBookAuthenticationSchemes.Development);

        Assert.True(result.Succeeded);
        Assert.Equal("AzureAD\\DavidKuziara", result.Principal?.Identity?.Name);
        Assert.False(result.Principal?.IsInRole(BuildBookRoles.Administrator));
    }

    private static ServiceProvider CreateProvider(string environmentName, bool useDevelopmentAuthentication)
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [$"{BuildBookOptions.SectionName}:Authorization:UseDevelopmentAuthentication"] = useDevelopmentAuthentication.ToString(),
                [$"{BuildBookOptions.SectionName}:Authorization:DevelopmentUserName"] = "AzureAD\\DavidKuziara"
            })
            .Build();

        services.AddLogging();
        services.AddOptions<BuildBookOptions>()
            .Bind(configuration.GetSection(BuildBookOptions.SectionName));
        services.AddBuildBookAuthentication(new TestHostEnvironment(environmentName), configuration);

        return services.BuildServiceProvider();
    }

    private sealed class TestHostEnvironment(string environmentName) : IWebHostEnvironment
    {
        public string EnvironmentName { get; set; } = environmentName;

        public string ApplicationName { get; set; } = "BuildBook.Tests";

        public string WebRootPath { get; set; } = AppContext.BaseDirectory;

        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();

        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;

        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
