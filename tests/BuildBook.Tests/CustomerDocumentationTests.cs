namespace BuildBook.Tests;

public class CustomerDocumentationTests
{
    [Fact]
    public void ReadmeLinksToCustomerSpecificationAndGuide()
    {
        var readmeContent = File.ReadAllText(Path.Combine(GetRepositoryRoot(), "README.md"));

        Assert.Contains("BuildBook Customer & Support Contracts Specification", readmeContent, StringComparison.Ordinal);
        Assert.Contains("docs/customer-module-guide.md", readmeContent, StringComparison.Ordinal);
        Assert.Contains("docs/customer-uat-checklist.md", readmeContent, StringComparison.Ordinal);
    }

    [Fact]
    public void AgentsInstructionsDirectCustomerWorkToCustomerSpecification()
    {
        var agentsContent = File.ReadAllText(Path.Combine(GetRepositoryRoot(), "AGENTS.md"));

        Assert.Contains("For Customer & Support Contracts module work, also read:", agentsContent, StringComparison.Ordinal);
        Assert.Contains("docs/specs/BuildBook-Customer-Support-Contracts-Specification.md", agentsContent, StringComparison.Ordinal);
    }

    [Fact]
    public void CustomerGuideDocumentsReportsSupportTicketsAndPriorityGuidance()
    {
        var guidePath = Path.Combine(GetRepositoryRoot(), "docs", "customer-module-guide.md");

        Assert.True(File.Exists(guidePath), "The customer module guide should exist.");

        var guideContent = File.ReadAllText(guidePath);

        Assert.Contains("Customers", guideContent, StringComparison.Ordinal);
        Assert.Contains("Support Ticket No.", guideContent, StringComparison.Ordinal);
        Assert.Contains("Customer Reports", guideContent, StringComparison.Ordinal);
        Assert.Contains("priority-mismatch", guideContent, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Build Record secrets", guideContent, StringComparison.Ordinal);
    }

    [Fact]
    public void CustomerUatChecklistCoversSharedLookupReportsExportsAndSecurity()
    {
        var checklistPath = Path.Combine(GetRepositoryRoot(), "docs", "customer-uat-checklist.md");

        Assert.True(File.Exists(checklistPath), "The customer module UAT checklist should exist.");

        var checklistContent = File.ReadAllText(checklistPath);

        Assert.Contains("shared customer list", checklistContent, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Support Ticket No.", checklistContent, StringComparison.Ordinal);
        Assert.Contains("Customers > Reports", checklistContent, StringComparison.Ordinal);
        Assert.Contains("Export", checklistContent, StringComparison.Ordinal);
        Assert.Contains("Build Record secrets", checklistContent, StringComparison.Ordinal);
    }

    private static string GetRepositoryRoot()
    {
        return Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
    }
}
