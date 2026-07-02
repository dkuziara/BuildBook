using BuildBook.Application.Paging;

namespace BuildBook.Application.Products;

public interface IProductService
{
    Task<PagedResult<ProductListItem>> SearchPageAsync(
        ProductListFilter filter,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ProductListItem>> SearchAsync(
        ProductListFilter filter,
        CancellationToken cancellationToken = default);

    Task<ProductDetailModel?> GetDetailAsync(
        int productId,
        CancellationToken cancellationToken = default);

    Task<ProductSaveResult> CreateAsync(
        CreateProductRequest request,
        string createdBy,
        CancellationToken cancellationToken = default);

    Task<ProductSaveResult> UpdateAsync(
        int productId,
        UpdateProductRequest request,
        string updatedBy,
        CancellationToken cancellationToken = default);
}
