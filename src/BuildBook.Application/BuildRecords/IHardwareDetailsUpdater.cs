namespace BuildBook.Application.BuildRecords;

public interface IHardwareDetailsUpdater
{
    Task<UpdateHardwareDetailsResult> UpdateAsync(
        int buildRecordId,
        UpdateHardwareDetailsRequest request,
        string updatedBy,
        CancellationToken cancellationToken = default);
}
