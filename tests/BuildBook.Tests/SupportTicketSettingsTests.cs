using BuildBook.Application.Settings;

namespace BuildBook.Tests;

public class SupportTicketSettingsTests
{
    [Fact]
    public void ValidatorRejectsUnsafeTemplateAndRequiresPlaceholder()
    {
        var result = SupportTicketSettingsValidator.Validate(new UpdateSupportTicketSettingsRequest
        {
            SupportTicketLabel = "Support Ticket No.",
            SupportTicketUrlTemplate = "javascript:alert(1)"
        });

        Assert.Contains("Support site URL template must contain {1}.", result);
        Assert.Contains("Support site URL template must start with http:// or https://.", result);
    }

    [Fact]
    public void LinkBuilderEncodesTicketNumberAndBuildsHttpUrl()
    {
        var url = SupportTicketLinkBuilder.BuildUrl(
            "https://support.example.com/tickets/{1}",
            "ABC 123");

        Assert.Equal("https://support.example.com/tickets/ABC%20123", url);
    }
}
