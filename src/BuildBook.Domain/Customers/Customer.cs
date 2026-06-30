using BuildBook.Domain.BuildRecords;
using BuildBook.Domain.Rmas;

namespace BuildBook.Domain.Customers;

public sealed class Customer
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? AccountCode { get; set; }

    public string? AddressLine1 { get; set; }

    public string? AddressLine2 { get; set; }

    public string? TownCity { get; set; }

    public string? CountyRegion { get; set; }

    public string? Postcode { get; set; }

    public string? Country { get; set; }

    public string? MainPhone { get; set; }

    public string? MainEmail { get; set; }

    public string? Website { get; set; }

    public string? PrimaryContactName { get; set; }

    public string? PrimaryContactEmail { get; set; }

    public string? PrimaryContactPhone { get; set; }

    public int? SupportContractLevelId { get; set; }

    public SupportContractLevel? SupportContractLevel { get; set; }

    public string SupportContractStatus { get; set; } = CustomerSupportContractStatuses.NoContract;

    public DateOnly? SupportContractStartDate { get; set; }

    public DateOnly? SupportContractEndDate { get; set; }

    public string? SupportNotes { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public string CreatedBy { get; set; } = string.Empty;

    public DateTimeOffset LastUpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public string LastUpdatedBy { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public ICollection<BuildRecord> BuildRecords { get; } = new List<BuildRecord>();

    public ICollection<RmaRecord> RmaRecords { get; } = new List<RmaRecord>();
}
