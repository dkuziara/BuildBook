using BuildBook.Domain.Orders;

namespace BuildBook.Application.Orders;

public interface IOrderWorkflowService
{
    Task<OrderOperationResult> UpdateWorkflowAsync(
        int orderRecordId,
        UpdateOrderWorkflowRequest request,
        string updatedBy,
        CancellationToken cancellationToken = default);

    Task<ChangeOrderStatusResult> ChangeStatusAsync(
        int orderRecordId,
        ChangeOrderStatusRequest request,
        string updatedBy,
        CancellationToken cancellationToken = default);

    Task<OrderOperationResult> SaveAssignmentAsync(
        int orderRecordId,
        SaveOrderAssignmentRequest request,
        string updatedBy,
        CancellationToken cancellationToken = default);

    Task<OrderOperationResult> DeleteAssignmentAsync(
        int orderRecordId,
        int assignmentId,
        string updatedBy,
        CancellationToken cancellationToken = default);

    Task<OrderOperationResult> UpdateChecklistItemAsync(
        int orderRecordId,
        UpdateOrderChecklistItemRequest request,
        string updatedBy,
        CancellationToken cancellationToken = default);
}
