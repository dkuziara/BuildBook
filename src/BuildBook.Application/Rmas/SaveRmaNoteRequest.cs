using BuildBook.Domain.Rmas;

namespace BuildBook.Application.Rmas;

public sealed class SaveRmaNoteRequest
{
    public int? NoteId { get; set; }

    public RmaNoteType NoteType { get; set; } = RmaNoteType.InternalNote;

    public string? NoteText { get; set; }
}
