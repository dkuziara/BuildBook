using BuildBook.Application.BuildRecords;

namespace BuildBook.Tests;

public class NetworkNotesTests
{
    [Fact]
    public void RequestStoresNetworkAndNotesFields()
    {
        var request = new UpdateNetworkNotesRequest
        {
            WifiSsid = "BuildBook-WiFi",
            RouterUsed = "Router A",
            Note = "General non-sensitive note."
        };

        Assert.Equal("BuildBook-WiFi", request.WifiSsid);
        Assert.Equal("Router A", request.RouterUsed);
        Assert.Equal("General non-sensitive note.", request.Note);
    }

    [Fact]
    public void FailureResultReturnsErrors()
    {
        var result = UpdateNetworkNotesResult.Failure("Build Record was not found.");

        Assert.False(result.Succeeded);
        Assert.Equal(["Build Record was not found."], result.Errors);
    }
}
