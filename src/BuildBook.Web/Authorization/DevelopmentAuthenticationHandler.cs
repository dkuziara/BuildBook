using System.Security.Claims;
using System.Security.Principal;
using System.Text.Encodings.Web;
using BuildBook.Web.Configuration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace BuildBook.Web.Authorization;

public sealed class DevelopmentAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    IOptions<BuildBookOptions> buildBookOptions)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "development-user"),
            new(ClaimTypes.Name, ResolveDevelopmentUserName())
        };

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }

    private string ResolveDevelopmentUserName()
    {
        if (!string.IsNullOrWhiteSpace(buildBookOptions.Value.Authorization.DevelopmentUserName))
        {
            return buildBookOptions.Value.Authorization.DevelopmentUserName.Trim();
        }

        if (OperatingSystem.IsWindows())
        {
            return WindowsIdentity.GetCurrent()?.Name
                ?? Environment.UserName
                ?? "Development User";
        }

        return Environment.UserName
            ?? "Development User";
    }
}
