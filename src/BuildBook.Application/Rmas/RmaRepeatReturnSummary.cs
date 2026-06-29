namespace BuildBook.Application.Rmas;

public sealed record RmaRepeatReturnSummary(
    int PreviousRmaCount,
    IReadOnlyList<RmaRegisterRow> PreviousRmas)
{
    public bool HasPreviousRmas => PreviousRmaCount > 0;

    public static RmaRepeatReturnSummary Empty { get; } = new(0, []);
}
