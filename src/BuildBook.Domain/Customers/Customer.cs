using BuildBook.Domain.BuildRecords;

namespace BuildBook.Domain.Customers;

public sealed class Customer
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Notes { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public string CreatedBy { get; set; } = string.Empty;

    public DateTimeOffset LastUpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public string LastUpdatedBy { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public ICollection<BuildRecord> BuildRecords { get; } = new List<BuildRecord>();
}
