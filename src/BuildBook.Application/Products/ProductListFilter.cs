namespace BuildBook.Application.Products;

public sealed class ProductListFilter
{
    public string? Search { get; set; }

    public ProductSortColumn SortBy { get; set; } = ProductSortColumn.ProductCode;

    public bool SortDescending { get; set; }

    public bool HasAnyFilter()
    {
        return !string.IsNullOrWhiteSpace(Search);
    }
}
