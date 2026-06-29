namespace BuildBook.Application.Rmas;

public sealed record RmaCommunicationModel(
    int Id,
    DateTimeOffset CommunicationDate,
    string ContactMethod,
    string? ContactPerson,
    string Summary,
    bool FollowUpRequired,
    DateOnly? FollowUpDate,
    string CreatedBy,
    DateTimeOffset CreatedAt);
