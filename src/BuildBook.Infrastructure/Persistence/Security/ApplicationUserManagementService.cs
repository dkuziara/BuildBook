using BuildBook.Application.Security;
using BuildBook.Domain.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace BuildBook.Infrastructure.Persistence.Security;

public sealed class ApplicationUserManagementService(
    IDbContextFactory<BuildBookDbContext> dbContextFactory,
    IConfiguration configuration) : IApplicationUserManagementService, IBuildBookRoleResolver
{
    public async Task<IReadOnlyList<ApplicationUserSummary>> ListAsync(CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        var bootstrapAdministrators = GetBootstrapAdministrators();
        var users = await dbContext.ApplicationUsers
            .AsNoTracking()
            .Include(user => user.UserRoles)
            .ThenInclude(userRole => userRole.ApplicationRole)
            .OrderBy(user => user.WindowsUserName)
            .ToListAsync(cancellationToken);

        return users
            .Select(user => new ApplicationUserSummary(
                user.Id,
                user.WindowsUserName,
                user.DisplayName,
                user.EmailAddress,
                user.IsActive,
                bootstrapAdministrators.Contains(user.WindowsUserName),
                [.. user.UserRoles
                    .Select(userRole => userRole.ApplicationRole!.Name)
                    .OrderBy(roleName => roleName, StringComparer.Ordinal)]))
            .ToArray();
    }

    public async Task<ApplicationUserCommandResult> CreateAsync(
        CreateApplicationUserRequest request,
        string createdBy,
        CancellationToken cancellationToken = default)
    {
        var windowsUserName = request.WindowsUserName.Trim();
        var displayName = NormalizeOptionalValue(request.DisplayName);
        var emailAddress = NormalizeOptionalValue(request.EmailAddress);
        var actor = NormalizeActor(createdBy);
        var validationErrors = ValidateWindowsUserName(windowsUserName);

        if (validationErrors.Count > 0)
        {
            return ApplicationUserCommandResult.Failure(validationErrors);
        }

        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        var existingUser = await dbContext.ApplicationUsers
            .AnyAsync(user => user.WindowsUserName == windowsUserName, cancellationToken);

        if (existingUser)
        {
            return ApplicationUserCommandResult.Failure("This Windows user has already been added.");
        }

        var user = new ApplicationUser
        {
            WindowsUserName = windowsUserName,
            DisplayName = displayName,
            EmailAddress = emailAddress,
            CreatedBy = actor,
            LastUpdatedBy = actor
        };

        await dbContext.ApplicationUsers.AddAsync(user, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ApplicationUserCommandResult.Success();
    }

    public async Task<ApplicationUserCommandResult> UpdateAsync(
        UpdateApplicationUserRequest request,
        string updatedBy,
        CancellationToken cancellationToken = default)
    {
        var displayName = NormalizeOptionalValue(request.DisplayName);
        var emailAddress = NormalizeOptionalValue(request.EmailAddress);
        var actor = NormalizeActor(updatedBy);
        var selectedRoles = request.AssignedRoles
            .Where(role => !string.IsNullOrWhiteSpace(role))
            .Select(role => role.Trim())
            .Distinct(StringComparer.Ordinal)
            .OrderBy(role => role, StringComparer.Ordinal)
            .ToArray();
        var validationErrors = selectedRoles
            .Where(role => !BuildBookRoles.All.Contains(role, StringComparer.Ordinal))
            .Select(role => $"'{role}' is not a valid BuildBook role.")
            .ToArray();

        if (validationErrors.Length > 0)
        {
            return ApplicationUserCommandResult.Failure(validationErrors);
        }

        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        await EnsureFixedRolesExistAsync(dbContext, cancellationToken);

        var user = await dbContext.ApplicationUsers
            .Include(applicationUser => applicationUser.UserRoles)
            .ThenInclude(userRole => userRole.ApplicationRole)
            .SingleOrDefaultAsync(applicationUser => applicationUser.Id == request.UserId, cancellationToken);

        if (user is null)
        {
            return ApplicationUserCommandResult.Failure("The selected user could not be found.");
        }

        var bootstrapAdministrators = GetBootstrapAdministrators();
        var isBootstrapAdministrator = bootstrapAdministrators.Contains(user.WindowsUserName);
        var isRemovingAdministratorRole = user.UserRoles.Any(userRole => userRole.ApplicationRole!.Name == BuildBookRoles.Administrator)
            && !selectedRoles.Contains(BuildBookRoles.Administrator, StringComparer.Ordinal);

        if (isBootstrapAdministrator && !request.IsActive)
        {
            return ApplicationUserCommandResult.Failure(
                "Configured bootstrap administrators cannot be deactivated while they remain in configuration.");
        }

        if (isBootstrapAdministrator && isRemovingAdministratorRole)
        {
            return ApplicationUserCommandResult.Failure(
                "Configured bootstrap administrators must retain the Administrator role while they remain in configuration.");
        }

        if (!request.IsActive && await WouldRemoveLastAdministratorAsync(
                dbContext,
                user,
                selectedRoles,
                request.IsActive,
                bootstrapAdministrators,
                cancellationToken))
        {
            return ApplicationUserCommandResult.Failure("The last Administrator cannot be deactivated.");
        }

        if (request.IsActive && isRemovingAdministratorRole && await WouldRemoveLastAdministratorAsync(
                dbContext,
                user,
                selectedRoles,
                request.IsActive,
                bootstrapAdministrators,
                cancellationToken))
        {
            return ApplicationUserCommandResult.Failure("The last Administrator role assignment cannot be removed.");
        }

        var rolesByName = await dbContext.ApplicationRoles
            .Where(role => selectedRoles.Contains(role.Name))
            .ToDictionaryAsync(role => role.Name, StringComparer.Ordinal, cancellationToken);

        if (rolesByName.Count != selectedRoles.Length)
        {
            return ApplicationUserCommandResult.Failure("BuildBook roles are not fully configured yet. Refresh the page and try again.");
        }

        user.DisplayName = displayName;
        user.EmailAddress = emailAddress;
        user.IsActive = request.IsActive;
        user.LastUpdatedAt = DateTimeOffset.UtcNow;
        user.LastUpdatedBy = actor;

        user.UserRoles.Clear();
        foreach (var roleName in selectedRoles)
        {
            user.UserRoles.Add(new ApplicationUserRole
            {
                ApplicationUserId = user.Id,
                ApplicationRoleId = rolesByName[roleName].Id
            });
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return ApplicationUserCommandResult.Success();
    }

    public async Task<IReadOnlyCollection<string>> GetEffectiveRolesAsync(
        string windowsUserName,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(windowsUserName))
        {
            return [];
        }

        var roles = new HashSet<string>(StringComparer.Ordinal);
        var trimmedUserName = windowsUserName.Trim();

        if (GetBootstrapAdministrators().Contains(trimmedUserName))
        {
            roles.Add(BuildBookRoles.Administrator);
        }

        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var user = await dbContext.ApplicationUsers
            .AsNoTracking()
            .Where(applicationUser => applicationUser.WindowsUserName == trimmedUserName && applicationUser.IsActive)
            .Select(applicationUser => new
            {
                Roles = applicationUser.UserRoles
                    .Select(userRole => userRole.ApplicationRole!.Name)
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (user is null)
        {
            return roles.ToArray();
        }

        foreach (var roleName in user.Roles)
        {
            roles.Add(roleName);
        }

        return roles.ToArray();
    }

    private HashSet<string> GetBootstrapAdministrators()
    {
        var bootstrapAdministrators = configuration.GetSection("BuildBook:Authorization:BootstrapAdministrators")
            .Get<string[]>();

        return bootstrapAdministrators?
            .Where(userName => !string.IsNullOrWhiteSpace(userName))
            .Select(userName => userName.Trim())
            .ToHashSet(StringComparer.Ordinal)
            ?? [];
    }

    private static List<string> ValidateWindowsUserName(string windowsUserName)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(windowsUserName))
        {
            errors.Add("Windows username is required.");
        }
        else if (!windowsUserName.Contains('\\'))
        {
            errors.Add("Windows username must include the domain or tenant, for example DOMAIN\\SomeUser.");
        }

        return errors;
    }

    private static string NormalizeActor(string? actor)
    {
        return string.IsNullOrWhiteSpace(actor) ? "Unknown" : actor.Trim();
    }

    private static string? NormalizeOptionalValue(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static bool UserHasAdministratorRole(ApplicationUser user)
    {
        return user.UserRoles.Any(userRole => userRole.ApplicationRole?.Name == BuildBookRoles.Administrator);
    }

    private static async Task<bool> WouldRemoveLastAdministratorAsync(
        BuildBookDbContext dbContext,
        ApplicationUser user,
        IReadOnlyCollection<string> selectedRoles,
        bool requestedIsActive,
        HashSet<string> bootstrapAdministrators,
        CancellationToken cancellationToken)
    {
        var effectiveAdministrators = bootstrapAdministrators.ToHashSet(StringComparer.Ordinal);

        var assignedAdministrators = await dbContext.ApplicationUsers
            .AsNoTracking()
            .Where(applicationUser => applicationUser.IsActive)
            .Where(applicationUser => applicationUser.UserRoles.Any(userRole => userRole.ApplicationRole!.Name == BuildBookRoles.Administrator))
            .Select(applicationUser => applicationUser.WindowsUserName)
            .ToListAsync(cancellationToken);

        foreach (var administrator in assignedAdministrators)
        {
            effectiveAdministrators.Add(administrator);
        }

        var currentUserIsEffectiveAdministrator = bootstrapAdministrators.Contains(user.WindowsUserName)
            || UserHasAdministratorRole(user) && user.IsActive;
        var userWillRemainAdministrator = bootstrapAdministrators.Contains(user.WindowsUserName)
            || requestKeepsAdministrator(selectedRoles);

        if (!currentUserIsEffectiveAdministrator)
        {
            return false;
        }

        if (!userWillRemainAdministrator || !requestedIsActive)
        {
            effectiveAdministrators.Remove(user.WindowsUserName);
            foreach (var bootstrapAdministrator in bootstrapAdministrators)
            {
                effectiveAdministrators.Add(bootstrapAdministrator);
            }
        }

        return effectiveAdministrators.Count == 0;

        static bool requestKeepsAdministrator(IReadOnlyCollection<string> roles)
        {
            return roles.Contains(BuildBookRoles.Administrator, StringComparer.Ordinal);
        }
    }

    private static async Task EnsureFixedRolesExistAsync(
        BuildBookDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var existingRoleNames = await dbContext.ApplicationRoles
            .Select(role => role.Name)
            .ToListAsync(cancellationToken);

        var missingRoles = BuildBookRoles.All
            .Except(existingRoleNames, StringComparer.Ordinal)
            .Select(roleName => new ApplicationRole
            {
                Name = roleName
            })
            .ToArray();

        if (missingRoles.Length == 0)
        {
            return;
        }

        await dbContext.ApplicationRoles.AddRangeAsync(missingRoles, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
