namespace BuildBook.Domain.Rmas;

public sealed class RmaCommunication
{
    public int Id { get; set; }

    public int RmaRecordId { get; set; }

    public RmaRecord? RmaRecord { get; set; }

    public DateTimeOffset CommunicationDate { get; set; } = DateTimeOffset.UtcNow;

    public string ContactMethod { get; set; } = string.Empty;

    public string? ContactPerson { get; set; }

    public string Summary { get; set; } = string.Empty;

    public bool FollowUpRequired { get; set; }

    public DateOnly? FollowUpDate { get; set; }

    public string CreatedBy { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
