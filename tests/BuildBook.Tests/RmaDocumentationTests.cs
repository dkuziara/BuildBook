namespace BuildBook.Tests;

public class RmaDocumentationTests
{
    [Fact]
    public void ReadmeLinksToRmaSpecification()
    {
        var readmeContent = File.ReadAllText(Path.Combine(GetRepositoryRoot(), "README.md"));

        Assert.Contains("BuildBook RMA Module Specification", readmeContent, StringComparison.Ordinal);
        Assert.Contains("docs/specs/BuildBook-RMA-Module-Specification.md", readmeContent, StringComparison.Ordinal);
    }

    [Fact]
    public void AgentsInstructionsDirectRmaWorkToRmaSpecification()
    {
        var agentsContent = File.ReadAllText(Path.Combine(GetRepositoryRoot(), "AGENTS.md"));

        Assert.Contains("For RMA module work, also read:", agentsContent, StringComparison.Ordinal);
        Assert.Contains("docs/specs/BuildBook-RMA-Module-Specification.md", agentsContent, StringComparison.Ordinal);
    }

    [Fact]
    public void RmaGuideDocumentsRegisterBoardReportsAndSecretHandling()
    {
        var guidePath = Path.Combine(GetRepositoryRoot(), "docs", "rma-module-guide.md");

        Assert.True(File.Exists(guidePath), "The RMA module guide should exist.");

        var guideContent = File.ReadAllText(guidePath);

        Assert.Contains("RMA Register", guideContent, StringComparison.Ordinal);
        Assert.Contains("Board view", guideContent, StringComparison.Ordinal);
        Assert.Contains("Reports", guideContent, StringComparison.Ordinal);
        Assert.Contains("Build Record secrets", guideContent, StringComparison.Ordinal);
    }

    [Fact]
    public void PlannerMigrationGuideDocumentsManualRecreationTraceabilityAndCutover()
    {
        var guidePath = Path.Combine(GetRepositoryRoot(), "docs", "rma-planner-migration-guide.md");

        Assert.True(File.Exists(guidePath), "The RMA Planner migration guide should exist.");

        var guideContent = File.ReadAllText(guidePath);

        Assert.Contains("manual recreation", guideContent, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("original Planner task title", guideContent, StringComparison.Ordinal);
        Assert.Contains("original Planner notes", guideContent, StringComparison.Ordinal);
        Assert.Contains("authoritative", guideContent, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void RmaUatChecklistCoversCreateEditChecklistStatusAndProtection()
    {
        var checklistPath = Path.Combine(GetRepositoryRoot(), "docs", "rma-uat-checklist.md");

        Assert.True(File.Exists(checklistPath), "The RMA UAT checklist should exist.");

        var checklistContent = File.ReadAllText(checklistPath);

        Assert.Contains("Create a new RMA", checklistContent, StringComparison.Ordinal);
        Assert.Contains("Planner migration traceability", checklistContent, StringComparison.Ordinal);
        Assert.Contains("Mark a checklist item complete", checklistContent, StringComparison.Ordinal);
        Assert.Contains("Change status", checklistContent, StringComparison.Ordinal);
        Assert.Contains("secrets are not exposed", checklistContent, StringComparison.OrdinalIgnoreCase);
    }

    private static string GetRepositoryRoot()
    {
        return Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
    }
}
