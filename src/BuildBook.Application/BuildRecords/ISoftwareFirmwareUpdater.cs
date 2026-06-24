namespace BuildBook.Application.BuildRecords;

public interface ISoftwareFirmwareUpdater
{
    Task<UpdateSoftwareFirmwareResult> UpdateAsync(
        int buildRecordId,
        UpdateSoftwareFirmwareRequest request,
        string updatedBy,
        CancellationToken cancellationToken = default);
}
