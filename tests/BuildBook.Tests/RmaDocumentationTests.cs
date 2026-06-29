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

    private static string GetRepositoryRoot()
    {
        return Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
    }
}
