using BuildBook.Application.Security;
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

        return services;
    }
}
