using BuildBook.Web.Configuration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Negotiate;

namespace BuildBook.Web.Authorization;

public static class AuthenticationServiceCollectionExtensions
{
    public static IServiceCollection AddBuildBookAuthentication(
        this IServiceCollection services,
        IWebHostEnvironment environment,
        IConfiguration configuration)
    {
        var useDevelopmentAuthentication = environment.IsDevelopment()
            && configuration.GetValue<bool>(
                $"{BuildBookOptions.SectionName}:Authorization:UseDevelopmentAuthentication");

        if (useDevelopmentAuthentication)
        {
            services.AddAuthentication(BuildBookAuthenticationSchemes.Development)
                .AddScheme<AuthenticationSchemeOptions, DevelopmentAuthenticationHandler>(
                    BuildBookAuthenticationSchemes.Development,
                    options => { });

            return services;
        }

        services.AddAuthentication(NegotiateDefaults.AuthenticationScheme)
            .AddNegotiate();

        return services;
    }
}
