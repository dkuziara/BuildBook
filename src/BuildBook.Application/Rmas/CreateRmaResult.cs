namespace BuildBook.Application.Rmas;

public sealed class CreateRmaResult
{
    private CreateRmaResult(bool succeeded, int? rmaRecordId, IReadOnlyList<string> errors)
    {
        Succeeded = succeeded;
        RmaRecordId = rmaRecordId;
        Errors = errors;
    }

    public bool Succeeded { get; }

    public int? RmaRecordId { get; }

    public IReadOnlyList<string> Errors { get; }

    public static CreateRmaResult Success(int rmaRecordId)
    {
        return new CreateRmaResult(true, rmaRecordId, []);
    }

    public static CreateRmaResult Failure(params string[] errors)
    {
        return new CreateRmaResult(false, null, errors);
    }
}
