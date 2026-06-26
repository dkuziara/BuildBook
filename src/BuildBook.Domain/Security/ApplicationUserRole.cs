namespace BuildBook.Domain.Security;

public sealed class ApplicationUserRole
{
    public int ApplicationUserId { get; set; }

    public ApplicationUser? ApplicationUser { get; set; }

    public int ApplicationRoleId { get; set; }

    public ApplicationRole? ApplicationRole { get; set; }
}
