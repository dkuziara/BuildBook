using BuildBook.Domain.Rmas;

namespace BuildBook.Application.Rmas;

public sealed record RmaNoteModel(
    int Id,
    RmaNoteType NoteType,
    string NoteText,
    string CreatedBy,
    DateTimeOffset CreatedAt,
    string? LastUpdatedBy,
    DateTimeOffset? LastUpdatedAt);
