using System.ComponentModel.DataAnnotations;
using BuildBook.Domain.Customers;
using BuildBook.Domain.Rmas;

namespace BuildBook.Application.Customers;

public sealed class CreateSupportContractLevelRequest
{
    [Required(ErrorMessage = "Level name is required.")]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int? TargetResponseTimeValue { get; set; }

    public SupportResponseTimeUnit? TargetResponseTimeUnit { get; set; }

    public RmaPriority? DefaultRmaPriority { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Priority weight must be zero or greater.")]
    public int RmaPriorityWeight { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Display order must be zero or greater.")]
    public int DisplayOrder { get; set; }

    public bool IsActive { get; set; } = true;
}
