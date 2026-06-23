namespace BuildBook.Application.BuildRecords;

public sealed class CreateBuildRecordResult
{
    private CreateBuildRecordResult(bool succeeded, int? buildRecordId, IReadOnlyList<string> errors)
    {
        Succeeded = succeeded;
        BuildRecordId = buildRecordId;
        Errors = errors;
    }

    public bool Succeeded { get; }

    public int? BuildRecordId { get; }

    public IReadOnlyList<string> Errors { get; }

    public static CreateBuildRecordResult Success(int buildRecordId)
    {
        return new CreateBuildRecordResult(true, buildRecordId, []);
    }

    public static CreateBuildRecordResult Failure(params string[] errors)
    {
        return new CreateBuildRecordResult(false, null, errors);
    }
}
