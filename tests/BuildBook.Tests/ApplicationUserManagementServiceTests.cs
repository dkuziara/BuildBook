using BuildBook.Application.Security;
using BuildBook.Domain.Security;
using BuildBook.Infrastructure.Persistence;
using BuildBook.Infrastructure.Persistence.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;

namespace BuildBook.Tests;

public class ApplicationUserManagementServiceTests
{
    [Fact]
    public async Task GetEffectiveRolesAsync_IncludesBootstrapAdministratorWithoutDatabaseUser()
    {
        var options = DatabaseTestHelper.CreateSqlServerOptions(nameof(GetEffectiveRolesAsync_IncludesBootstrapAdministratorWithoutDatabaseUser));
        await DatabaseTestHelper.InitializeDatabaseAsync(options);

        try
        {
            var service = CreateService(options, bootstrapAdministrators: ["AzureAD\\BootstrapAdmin"]);

            var roles = await service.GetEffectiveRolesAsync("AzureAD\\BootstrapAdmin");

            Assert.Contains(BuildBookRoles.Administrator, roles);
        }
        finally
        {
            await DatabaseTestHelper.DeleteDatabaseAsync(options);
        }
    }

    [Fact]
    public async Task CreateAsync_AddsWindowsUser()
    {
        var options = DatabaseTestHelper.CreateSqlServerOptions(nameof(CreateAsync_AddsWindowsUser));
        await DatabaseTestHelper.InitializeDatabaseAsync(options);

        try
        {
            var service = CreateManagementService(options);

            var result = await service.CreateAsync(
                new CreateApplicationUserRequest("AzureAD\\NewUser", "New User", "new.user@example.com"),
                "AzureAD\\Admin");

            Assert.True(result.Succeeded);

            await using var verifyContext = new BuildBookDbContext(options);
            var user = await verifyContext.ApplicationUsers.SingleAsync();

            Assert.Equal("AzureAD\\NewUser", user.WindowsUserName);
            Assert.Equal("New User", user.DisplayName);
            Assert.Equal("new.user@example.com", user.EmailAddress);
            Assert.True(user.IsActive);
        }
        finally
        {
            await DatabaseTestHelper.DeleteDatabaseAsync(options);
        }
    }

    [Fact]
    public async Task UpdateAsync_AssignsRequestedRoles()
    {
        var options = DatabaseTestHelper.CreateSqlServerOptions(nameof(UpdateAsync_AssignsRequestedRoles));
        await DatabaseTestHelper.InitializeDatabaseAsync(options);

        try
        {
            await using (var setupContext = new BuildBookDbContext(options))
            {
                setupContext.ApplicationUsers.Add(new ApplicationUser
                {
                    WindowsUserName = "AzureAD\\EditorUser",
                    CreatedBy = "Test",
                    LastUpdatedBy = "Test"
                });

                await setupContext.SaveChangesAsync();
            }

            var service = CreateManagementService(options);

            await using (var roleSeedContext = new BuildBookDbContext(options))
            {
                if (!await roleSeedContext.ApplicationRoles.AnyAsync())
                {
                    roleSeedContext.ApplicationRoles.AddRange(BuildBookRoles.All.Select(roleName => new ApplicationRole
                    {
                        Name = roleName
                    }));
                    await roleSeedContext.SaveChangesAsync();
                }
            }

            await using var loadContext = new BuildBookDbContext(options);
            var userId = await loadContext.ApplicationUsers.Select(user => user.Id).SingleAsync();

            var result = await service.UpdateAsync(
                new UpdateApplicationUserRequest(
                    userId,
                    "Editor User",
                    "editor.user@example.com",
                    true,
                    [BuildBookRoles.Editor, BuildBookRoles.Viewer]),
                "AzureAD\\Admin");

            Assert.True(result.Succeeded);

            await using var verifyContext = new BuildBookDbContext(options);
            var user = await verifyContext.ApplicationUsers
                .Include(applicationUser => applicationUser.UserRoles)
                .ThenInclude(userRole => userRole.ApplicationRole)
                .SingleAsync();

            Assert.Equal("Editor User", user.DisplayName);
            Assert.Equal("editor.user@example.com", user.EmailAddress);
            Assert.Equal(
                [BuildBookRoles.Editor, BuildBookRoles.Viewer],
                user.UserRoles.Select(userRole => userRole.ApplicationRole!.Name).OrderBy(role => role, StringComparer.Ordinal).ToArray());
        }
        finally
        {
            await DatabaseTestHelper.DeleteDatabaseAsync(options);
        }
    }

    [Fact]
    public async Task UpdateAsync_PreventsRemovingLastAdministratorRoleAssignment()
    {
        var options = DatabaseTestHelper.CreateSqlServerOptions(nameof(UpdateAsync_PreventsRemovingLastAdministratorRoleAssignment));
        await DatabaseTestHelper.InitializeDatabaseAsync(options);

        try
        {
            await SeedUserWithAdministratorRoleAsync(options, "AzureAD\\AdminUser");
            var service = CreateManagementService(options);

            await using var context = new BuildBookDbContext(options);
            var userId = await context.ApplicationUsers.Select(user => user.Id).SingleAsync();

            var result = await service.UpdateAsync(
                new UpdateApplicationUserRequest(userId, "Admin User", null, true, []),
                "AzureAD\\AdminUser");

            Assert.False(result.Succeeded);
            Assert.Contains("last Administrator role assignment", result.Errors.Single(), StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            await DatabaseTestHelper.DeleteDatabaseAsync(options);
        }
    }

    [Fact]
    public async Task UpdateAsync_PreventsDeactivatingLastAdministrator()
    {
        var options = DatabaseTestHelper.CreateSqlServerOptions(nameof(UpdateAsync_PreventsDeactivatingLastAdministrator));
        await DatabaseTestHelper.InitializeDatabaseAsync(options);

        try
        {
            await SeedUserWithAdministratorRoleAsync(options, "AzureAD\\AdminUser");
            var service = CreateManagementService(options);

            await using var context = new BuildBookDbContext(options);
            var userId = await context.ApplicationUsers.Select(user => user.Id).SingleAsync();

            var result = await service.UpdateAsync(
                new UpdateApplicationUserRequest(userId, "Admin User", null, false, [BuildBookRoles.Administrator]),
                "AzureAD\\AdminUser");

            Assert.False(result.Succeeded);
            Assert.Contains("last Administrator cannot be deactivated", result.Errors.Single(), StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            await DatabaseTestHelper.DeleteDatabaseAsync(options);
        }
    }

    private static ApplicationUserManagementService CreateService(
        DbContextOptions<BuildBookDbContext> options,
        string[]? bootstrapAdministrators = null)
    {
        var configurationValues = (bootstrapAdministrators ?? [])
            .Select((userName, index) => new KeyValuePair<string, string?>(
                $"BuildBook:Authorization:BootstrapAdministrators:{index}",
                userName))
            .ToDictionary();

        return new ApplicationUserManagementService(
            new TestDbContextFactory(options),
            new ConfigurationBuilder()
                .AddInMemoryCollection(configurationValues)
                .Build());
    }

    private static IApplicationUserManagementService CreateManagementService(DbContextOptions<BuildBookDbContext> options)
    {
        return CreateService(options);
    }

    private static async Task SeedUserWithAdministratorRoleAsync(
        DbContextOptions<BuildBookDbContext> options,
        string windowsUserName)
    {
        await using var context = new BuildBookDbContext(options);

        var adminRole = new ApplicationRole
        {
            Name = BuildBookRoles.Administrator
        };

        var user = new ApplicationUser
        {
            WindowsUserName = windowsUserName,
            DisplayName = "Admin User",
            CreatedBy = "Test",
            LastUpdatedBy = "Test"
        };
        user.UserRoles.Add(new ApplicationUserRole
        {
            ApplicationRole = adminRole
        });

        context.ApplicationRoles.Add(adminRole);
        context.ApplicationUsers.Add(user);
        await context.SaveChangesAsync();
    }
}
