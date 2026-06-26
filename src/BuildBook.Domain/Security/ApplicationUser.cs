namespace BuildBook.Domain.Security;

public sealed class ApplicationUser
{
    public int Id { get; set; }

    public string WindowsUserName { get; set; } = string.Empty;

    public string? DisplayName { get; set; }

    public string? EmailAddress { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public string CreatedBy { get; set; } = string.Empty;

    public DateTimeOffset LastUpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public string LastUpdatedBy { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public ICollection<ApplicationUserRole> UserRoles { get; } = new List<ApplicationUserRole>();
}
