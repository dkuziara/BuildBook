namespace BuildBook.Tests;

public class OperationsDocumentationTests
{
    [Fact]
    public void BackupAndRestoreGuideDocumentsSqlServerRestoreAndKeyBackups()
    {
        var guidePath = Path.Combine(
            GetRepositoryRoot(),
            "docs",
            "backup-and-restore-guide.md");

        Assert.True(File.Exists(guidePath), "The backup and restore guide should exist.");

        var guideContent = File.ReadAllText(guidePath);

        Assert.Contains("SQL Server", guideContent, StringComparison.Ordinal);
        Assert.Contains("BACKUP DATABASE", guideContent, StringComparison.Ordinal);
        Assert.Contains("RESTORE DATABASE", guideContent, StringComparison.Ordinal);
        Assert.Contains("Data Protection key", guideContent, StringComparison.Ordinal);
        Assert.Contains("Admin > Users & Roles", guideContent, StringComparison.Ordinal);
        Assert.Contains("sensitive", guideContent, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void UatChecklistCoversImportSearchEditSecretsReportsAndExport()
    {
        var checklistPath = Path.Combine(
            GetRepositoryRoot(),
            "docs",
            "uat-checklist.md");

        Assert.True(File.Exists(checklistPath), "The UAT checklist should exist.");

        var checklistContent = File.ReadAllText(checklistPath);

        Assert.Contains("Import", checklistContent, StringComparison.Ordinal);
        Assert.Contains("Search", checklistContent, StringComparison.Ordinal);
        Assert.Contains("Edit", checklistContent, StringComparison.Ordinal);
        Assert.Contains("Secrets", checklistContent, StringComparison.Ordinal);
        Assert.Contains("Reports", checklistContent, StringComparison.Ordinal);
        Assert.Contains("Export", checklistContent, StringComparison.Ordinal);
        Assert.Contains("Admin > Users & Roles", checklistContent, StringComparison.Ordinal);
    }

    [Fact]
    public void SpreadsheetRetirementPlanDocumentsReadOnlyOrArchiveTransition()
    {
        var planPath = Path.Combine(
            GetRepositoryRoot(),
            "docs",
            "spreadsheet-retirement-plan.md");

        Assert.True(File.Exists(planPath), "The spreadsheet retirement plan should exist.");

        var planContent = File.ReadAllText(planPath);

        Assert.Contains("read-only", planContent, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("archived", planContent, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("BuildBook is now the primary system", planContent, StringComparison.Ordinal);
        Assert.Contains("final verified spreadsheet import", planContent, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("historical reference", planContent, StringComparison.OrdinalIgnoreCase);
    }

    private static string GetRepositoryRoot()
    {
        return Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
    }
}
