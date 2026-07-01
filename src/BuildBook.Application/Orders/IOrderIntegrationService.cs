namespace BuildBook.Application.Orders;

public interface IOrderIntegrationService
{
    Task<OrderOperationResult> UpdateCustomerAndSupportAsync(
        int orderRecordId,
        UpdateOrderCustomerAndSupportRequest request,
        string updatedBy,
        CancellationToken cancellationToken = default);

    Task<OrderOperationResult> LinkBuildRecordAsync(
        int orderRecordId,
        int buildRecordId,
        string updatedBy,
        CancellationToken cancellationToken = default);

    Task<OrderOperationResult> UnlinkBuildRecordAsync(
        int orderRecordId,
        int buildRecordId,
        string updatedBy,
        CancellationToken cancellationToken = default);
}
