namespace BuildBook.Application.BuildRecords;

public interface INetworkNotesUpdater
{
    Task<UpdateNetworkNotesResult> UpdateAsync(
        int buildRecordId,
        UpdateNetworkNotesRequest request,
        string updatedBy,
        CancellationToken cancellationToken = default);
}
