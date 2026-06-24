namespace BuildBook.Application.BuildRecords;

public interface IProductDetailsUpdater
{
    Task<UpdateProductDetailsResult> UpdateAsync(
        int buildRecordId,
        UpdateProductDetailsRequest request,
        string updatedBy,
        CancellationToken cancellationToken = default);
}
