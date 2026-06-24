namespace BuildBook.Application.BuildRecords;

public interface ICustomerShippingUpdater
{
    Task<UpdateCustomerShippingResult> UpdateAsync(
        int buildRecordId,
        UpdateCustomerShippingRequest request,
        string updatedBy,
        CancellationToken cancellationToken = default);
}
