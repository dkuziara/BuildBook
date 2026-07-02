using System.ComponentModel.DataAnnotations;

namespace BuildBook.Application.Products;

public sealed class CreateProductRequest
{
    [Required(ErrorMessage = "Product code is required.")]
    [StringLength(64, ErrorMessage = "Product code must be 64 characters or fewer.")]
    public string ProductCode { get; set; } = string.Empty;

    [StringLength(256, ErrorMessage = "Description must be 256 characters or fewer.")]
    public string? Description { get; set; }

    [StringLength(4000, ErrorMessage = "Notes must be 4000 characters or fewer.")]
    public string? Notes { get; set; }
}
