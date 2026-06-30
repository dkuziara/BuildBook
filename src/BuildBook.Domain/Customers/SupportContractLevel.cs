using BuildBook.Domain.Rmas;

namespace BuildBook.Domain.Customers;

public sealed class SupportContractLevel
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int? TargetResponseTimeValue { get; set; }

    public SupportResponseTimeUnit? TargetResponseTimeUnit { get; set; }

    public RmaPriority? DefaultRmaPriority { get; set; }

    public int RmaPriorityWeight { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public string CreatedBy { get; set; } = string.Empty;

    public DateTimeOffset LastUpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public string LastUpdatedBy { get; set; } = string.Empty;

    public ICollection<Customer> Customers { get; } = new List<Customer>();
}
