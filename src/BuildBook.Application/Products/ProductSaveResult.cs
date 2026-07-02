namespace BuildBook.Application.Products;

public sealed class ProductSaveResult
{
    private ProductSaveResult(bool succeeded, int? productId, IReadOnlyList<string> errors)
    {
        Succeeded = succeeded;
        ProductId = productId;
        Errors = errors;
    }

    public bool Succeeded { get; }

    public int? ProductId { get; }

    public IReadOnlyList<string> Errors { get; }

    public static ProductSaveResult Success(int productId) => new(true, productId, []);

    public static ProductSaveResult Failure(params string[] errors) => new(false, null, errors);
}
