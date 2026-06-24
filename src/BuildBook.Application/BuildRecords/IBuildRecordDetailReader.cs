namespace BuildBook.Application.BuildRecords;

public interface IBuildRecordDetailReader
{
    Task<BuildRecordDetailModel?> GetByIdAsync(
        int buildRecordId,
        CancellationToken cancellationToken = default);
}
