using BuildBook.Application.Products;
using BuildBook.Domain.Products;
using Microsoft.EntityFrameworkCore;

namespace BuildBook.Infrastructure.Persistence.Products;

public sealed class ProductService(IDbContextFactory<BuildBookDbContext> dbContextFactory) : IProductService
{
    public async Task<IReadOnlyList<ProductListItem>> SearchAsync(
        ProductListFilter filter,
        CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        var query = dbContext.Products
            .AsNoTracking()
            .AsQueryable();

        var search = NormalizeOptionalValue(filter.Search);
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(product =>
                product.ProductCode.Contains(search)
                || (product.Description != null && product.Description.Contains(search))
                || (product.Notes != null && product.Notes.Contains(search)));
        }

        query = ApplySort(query, filter);

        return await query
            .Select(product => new ProductListItem(
                product.Id,
                product.ProductCode,
                product.Description,
                product.Notes,
                product.LastUpdatedAt))
            .ToListAsync(cancellationToken);
    }

    public async Task<ProductDetailModel?> GetDetailAsync(
        int productId,
        CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        return await dbContext.Products
            .AsNoTracking()
            .Where(product => product.Id == productId)
            .Select(product => new ProductDetailModel(
                product.Id,
                product.ProductCode,
                product.Description,
                product.Notes,
                product.CreatedAt,
                product.CreatedBy,
                product.LastUpdatedAt,
                product.LastUpdatedBy))
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<ProductSaveResult> CreateAsync(
        CreateProductRequest request,
        string createdBy,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.ProductCode))
        {
            return ProductSaveResult.Failure("Product code is required.");
        }

        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var normalizedProductCode = request.ProductCode.Trim();

        if (await ProductCodeExistsAsync(dbContext, normalizedProductCode, null, cancellationToken))
        {
            return ProductSaveResult.Failure("A product with this code already exists.");
        }

        var userName = NormalizeUserName(createdBy);
        var product = new Product
        {
            ProductCode = normalizedProductCode,
            Description = NormalizeOptionalValue(request.Description),
            Notes = NormalizeOptionalValue(request.Notes),
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = userName,
            LastUpdatedAt = DateTimeOffset.UtcNow,
            LastUpdatedBy = userName
        };

        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ProductSaveResult.Success(product.Id);
    }

    public async Task<ProductSaveResult> UpdateAsync(
        int productId,
        UpdateProductRequest request,
        string updatedBy,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.ProductCode))
        {
            return ProductSaveResult.Failure("Product code is required.");
        }

        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var product = await dbContext.Products
            .SingleOrDefaultAsync(existingProduct => existingProduct.Id == productId, cancellationToken);

        if (product is null)
        {
            return ProductSaveResult.Failure("Product was not found.");
        }

        var normalizedProductCode = request.ProductCode.Trim();
        if (await ProductCodeExistsAsync(dbContext, normalizedProductCode, productId, cancellationToken))
        {
            return ProductSaveResult.Failure("A product with this code already exists.");
        }

        product.ProductCode = normalizedProductCode;
        product.Description = NormalizeOptionalValue(request.Description);
        product.Notes = NormalizeOptionalValue(request.Notes);
        product.LastUpdatedAt = DateTimeOffset.UtcNow;
        product.LastUpdatedBy = NormalizeUserName(updatedBy);

        await dbContext.SaveChangesAsync(cancellationToken);

        return ProductSaveResult.Success(product.Id);
    }

    private static IQueryable<Product> ApplySort(IQueryable<Product> query, ProductListFilter filter)
    {
        return (filter.SortBy, filter.SortDescending) switch
        {
            (ProductSortColumn.Description, false) => query
                .OrderBy(product => product.Description)
                .ThenBy(product => product.ProductCode),
            (ProductSortColumn.Description, true) => query
                .OrderByDescending(product => product.Description)
                .ThenBy(product => product.ProductCode),
            (ProductSortColumn.LastUpdated, false) => query
                .OrderBy(product => product.LastUpdatedAt)
                .ThenBy(product => product.ProductCode),
            (ProductSortColumn.LastUpdated, true) => query
                .OrderByDescending(product => product.LastUpdatedAt)
                .ThenBy(product => product.ProductCode),
            (_, true) => query.OrderByDescending(product => product.ProductCode),
            _ => query.OrderBy(product => product.ProductCode)
        };
    }

    private static async Task<bool> ProductCodeExistsAsync(
        BuildBookDbContext dbContext,
        string normalizedProductCode,
        int? excludedProductId,
        CancellationToken cancellationToken)
    {
        return await dbContext.Products.AnyAsync(
            product => product.ProductCode.ToLower() == normalizedProductCode.ToLower()
                && (excludedProductId == null || product.Id != excludedProductId.Value),
            cancellationToken);
    }

    private static string? NormalizeOptionalValue(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static string NormalizeUserName(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? "Unknown" : value.Trim();
    }
}
