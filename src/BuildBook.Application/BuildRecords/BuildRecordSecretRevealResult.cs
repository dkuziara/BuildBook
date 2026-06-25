namespace BuildBook.Application.BuildRecords;

public sealed class BuildRecordSecretRevealResult
{
    private BuildRecordSecretRevealResult(bool succeeded, string? secretValue, IReadOnlyList<string> errors)
    {
        Succeeded = succeeded;
        SecretValue = secretValue;
        Errors = errors;
    }

    public bool Succeeded { get; }

    public string? SecretValue { get; }

    public IReadOnlyList<string> Errors { get; }

    public static BuildRecordSecretRevealResult Success(string? secretValue)
    {
        return new BuildRecordSecretRevealResult(true, secretValue, []);
    }

    public static BuildRecordSecretRevealResult Failure(params string[] errors)
    {
        return new BuildRecordSecretRevealResult(false, null, errors);
    }
}
