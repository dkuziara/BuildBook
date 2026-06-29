namespace BuildBook.Application.Rmas;

public sealed class SaveRmaCommunicationRequest
{
    public int? CommunicationId { get; set; }

    public DateOnly? CommunicationDate { get; set; }

    public string? ContactMethod { get; set; }

    public string? ContactPerson { get; set; }

    public string? Summary { get; set; }

    public bool FollowUpRequired { get; set; }

    public DateOnly? FollowUpDate { get; set; }
}
