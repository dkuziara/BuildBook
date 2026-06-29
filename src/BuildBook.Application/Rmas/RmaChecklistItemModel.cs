namespace BuildBook.Application.Rmas;

public sealed record RmaChecklistItemModel(
    int Id,
    int DisplayOrder,
    string Text,
    bool IsCompleted,
    string? CompletedBy,
    DateTimeOffset? CompletedAt,
    bool ShowInBoardView,
    bool IsCustom);
