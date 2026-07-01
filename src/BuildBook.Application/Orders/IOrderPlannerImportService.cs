namespace BuildBook.Application.Orders;

public interface IOrderPlannerImportService
{
    Task<OrderPlannerImportReview> BuildReviewAsync(
        string fileName,
        Stream fileStream,
        CancellationToken cancellationToken = default);

    Task<OrderPlannerImportValidationResult> BuildValidationAsync(
        string fileName,
        Stream fileStream,
        CancellationToken cancellationToken = default);

    Task<OrderPlannerImportExecutionResult> BuildImportAsync(
        string fileName,
        Stream fileStream,
        string importedBy,
        CancellationToken cancellationToken = default);
}
