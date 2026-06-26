namespace BuildBook.Application.Security;

public interface IBuildBookRoleResolver
{
    Task<IReadOnlyCollection<string>> GetEffectiveRolesAsync(
        string windowsUserName,
        CancellationToken cancellationToken = default);
}
