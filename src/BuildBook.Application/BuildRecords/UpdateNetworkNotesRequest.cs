namespace BuildBook.Application.BuildRecords;

public sealed class UpdateNetworkNotesRequest
{
    public string? WifiSsid { get; set; }

    public string? RouterUsed { get; set; }

    public string? Note { get; set; }
}
