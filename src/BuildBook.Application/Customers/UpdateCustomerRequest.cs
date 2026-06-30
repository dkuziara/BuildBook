using System.ComponentModel.DataAnnotations;

namespace BuildBook.Application.Customers;

public sealed class UpdateCustomerRequest
{
    [Required(ErrorMessage = "Customer name is required.")]
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

    public string SupportContractStatus { get; set; } = string.Empty;

    public DateOnly? SupportContractStartDate { get; set; }

    public DateOnly? SupportContractEndDate { get; set; }

    public string? SupportNotes { get; set; }

    public bool IsActive { get; set; } = true;
}
