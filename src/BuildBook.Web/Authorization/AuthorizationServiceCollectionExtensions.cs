using BuildBook.Application.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;

namespace BuildBook.Web.Authorization;

public static class AuthorizationServiceCollectionExtensions
{
    public static IServiceCollection AddBuildBookAuthorization(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.FallbackPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();

            options.AddPolicy(
                BuildBookPolicies.ViewBuildRecords,
                policy => policy.RequireRole(
                    BuildBookRoles.Viewer,
                    BuildBookRoles.Editor,
                    BuildBookRoles.Administrator));

            options.AddPolicy(
                BuildBookPolicies.EditBuildRecords,
                policy => policy.RequireRole(
                    BuildBookRoles.Editor,
                    BuildBookRoles.Administrator));

            options.AddPolicy(
                BuildBookPolicies.RevealSensitiveData,
                policy => policy.RequireRole(
                    BuildBookRoles.SensitiveDataViewer,
                    BuildBookRoles.Administrator));

            options.AddPolicy(
                BuildBookPolicies.ManageSensitiveData,
                policy => policy.RequireAssertion(context =>
                    context.User.IsInRole(BuildBookRoles.Administrator)
                    || (context.User.IsInRole(BuildBookRoles.Editor)
                        && context.User.IsInRole(BuildBookRoles.SensitiveDataViewer))));

            options.AddPolicy(
                BuildBookPolicies.ImportSpreadsheet,
                policy => policy.RequireRole(BuildBookRoles.Administrator));

            options.AddPolicy(
                BuildBookPolicies.ExportNonSensitiveData,
                policy => policy.RequireRole(
                    BuildBookRoles.Viewer,
                    BuildBookRoles.Editor,
                    BuildBookRoles.Administrator));

            options.AddPolicy(
                BuildBookPolicies.ManageUsers,
                policy => policy.RequireRole(BuildBookRoles.Administrator));

            options.AddPolicy(
                BuildBookPolicies.DeleteRecords,
                policy => policy.RequireRole(BuildBookRoles.Administrator));
        });
        services.AddScoped<IBuildBookPermissionService, BuildBookPermissionService>();
        services.AddTransient<IClaimsTransformation, BuildBookRoleClaimsTransformation>();

        return services;
    }
}
