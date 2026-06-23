namespace BuildBook.Application.BuildRecords;

public interface IBuildRecordCreator
{
    Task<CreateBuildRecordResult> CreateAsync(
        CreateBuildRecordRequest request,
        string createdBy,
        CancellationToken cancellationToken = default);
}
