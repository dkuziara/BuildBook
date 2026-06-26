namespace BuildBook.Domain.Security;

public sealed class ApplicationRole
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public ICollection<ApplicationUserRole> UserRoles { get; } = new List<ApplicationUserRole>();
}
