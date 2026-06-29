namespace BuildBook.Application.Rmas;

public sealed class SaveRmaPartResult
{
    private SaveRmaPartResult(bool succeeded, int? partId, IReadOnlyList<string> errors)
    {
        Succeeded = succeeded;
        PartId = partId;
        Errors = errors;
    }

    public bool Succeeded { get; }

    public int? PartId { get; }

    public IReadOnlyList<string> Errors { get; }

    public static SaveRmaPartResult Success(int? partId = null) => new(true, partId, []);

    public static SaveRmaPartResult Failure(params string[] errors) => new(false, null, errors);

    public static SaveRmaPartResult Failure(IReadOnlyList<string> errors) => new(false, null, errors);
}
