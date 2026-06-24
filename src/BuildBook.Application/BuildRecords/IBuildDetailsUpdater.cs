namespace BuildBook.Application.BuildRecords;

public interface IBuildDetailsUpdater
{
    Task<UpdateBuildDetailsResult> UpdateAsync(
        int buildRecordId,
        UpdateBuildDetailsRequest request,
        string updatedBy,
        CancellationToken cancellationToken = default);
}
