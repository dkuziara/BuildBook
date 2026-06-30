namespace BuildBook.Application.Customers;

public sealed class SupportContractLevelSaveResult
{
    private SupportContractLevelSaveResult(bool succeeded, int? supportContractLevelId, IReadOnlyList<string> errors)
    {
        Succeeded = succeeded;
        SupportContractLevelId = supportContractLevelId;
        Errors = errors;
    }

    public bool Succeeded { get; }

    public int? SupportContractLevelId { get; }

    public IReadOnlyList<string> Errors { get; }

    public static SupportContractLevelSaveResult Success(int supportContractLevelId)
    {
        return new SupportContractLevelSaveResult(true, supportContractLevelId, []);
    }

    public static SupportContractLevelSaveResult Failure(params string[] errors)
    {
        return new SupportContractLevelSaveResult(false, null, errors);
    }
}
