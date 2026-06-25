namespace BuildBook.Tests;

public class SecretSafetyTests
{
    [Fact]
    public void ReportsPageDoesNotReferenceSensitiveExports()
    {
        var reportsPagePath = Path.Combine(
            GetRepositoryRoot(),
            "src",
            "BuildBook.Web",
            "Components",
            "Pages",
            "Reports.razor");
        var reportsPageContent = File.ReadAllText(reportsPagePath);

        Assert.DoesNotContain("Password", reportsPageContent);
        Assert.DoesNotContain("BitLocker", reportsPageContent);
        Assert.DoesNotContain("RecoveryKey", reportsPageContent);
        Assert.DoesNotContain("BuildRecordSecret", reportsPageContent);
    }

    [Fact]
    public void CsvExporterDoesNotReferenceSensitiveFields()
    {
        var exporterPath = Path.Combine(
            GetRepositoryRoot(),
            "src",
            "BuildBook.Infrastructure",
            "Persistence",
            "BuildRecords",
            "BuildRegisterCsvExporter.cs");
        var exporterContent = File.ReadAllText(exporterPath);

        Assert.DoesNotContain("Password", exporterContent);
        Assert.DoesNotContain("BitLocker", exporterContent);
        Assert.DoesNotContain("RecoveryKey", exporterContent);
        Assert.DoesNotContain("BuildRecordSecret", exporterContent);
    }

    [Fact]
    public void SecretServiceDoesNotWriteSensitiveValuesToLogs()
    {
        var secretServicePath = Path.Combine(
            GetRepositoryRoot(),
            "src",
            "BuildBook.Infrastructure",
            "Persistence",
            "BuildRecords",
            "BuildRecordSecretService.cs");
        var secretServiceContent = File.ReadAllText(secretServicePath);

        Assert.DoesNotContain("ILogger", secretServiceContent);
        Assert.DoesNotContain("LogInformation", secretServiceContent);
        Assert.DoesNotContain("LogWarning", secretServiceContent);
        Assert.DoesNotContain("LogError", secretServiceContent);
        Assert.DoesNotContain("Console.Write", secretServiceContent);
    }

    [Fact]
    public void ApplicationLoggingDoesNotMentionSensitiveFields()
    {
        var programPath = Path.Combine(
            GetRepositoryRoot(),
            "src",
            "BuildBook.Web",
            "Program.cs");
        var initializerPath = Path.Combine(
            GetRepositoryRoot(),
            "src",
            "BuildBook.Infrastructure",
            "Persistence",
            "BuildBookDatabaseInitializer.cs");

        var programContent = File.ReadAllText(programPath);
        var initializerContent = File.ReadAllText(initializerPath);

        AssertDoesNotContainSensitiveTerms(programContent);
        AssertDoesNotContainSensitiveTerms(initializerContent);
    }

    [Fact]
    public void InfrastructureDisablesEntityFrameworkSensitiveDataLogging()
    {
        var dependencyInjectionPath = Path.Combine(
            GetRepositoryRoot(),
            "src",
            "BuildBook.Infrastructure",
            "DependencyInjection.cs");
        var dependencyInjectionContent = File.ReadAllText(dependencyInjectionPath);

        Assert.Contains("options.EnableSensitiveDataLogging(false);", dependencyInjectionContent);
    }

    [Fact]
    public void SearchAndAuditSafetyCoverageExists()
    {
        var searchTestsPath = Path.Combine(
            GetRepositoryRoot(),
            "tests",
            "BuildBook.Tests",
            "BuildRecordSearchTests.cs");
        var auditTestsPath = Path.Combine(
            GetRepositoryRoot(),
            "tests",
            "BuildBook.Tests",
            "BuildRecordAuditServiceTests.cs");

        var searchTestsContent = File.ReadAllText(searchTestsPath);
        var auditTestsContent = File.ReadAllText(auditTestsPath);

        Assert.Contains("SearchServiceDoesNotReferenceSensitiveFields", searchTestsContent);
        Assert.Contains("DoesNotStoreSensitiveFieldValues", auditTestsContent);
        Assert.Contains("SensitiveAuditEntries_DoNotStoreSecretValues", auditTestsContent);
    }

    private static void AssertDoesNotContainSensitiveTerms(string content)
    {
        Assert.DoesNotContain("Password", content);
        Assert.DoesNotContain("BitLocker", content);
        Assert.DoesNotContain("RecoveryKey", content);
        Assert.DoesNotContain("Router password", content);
        Assert.DoesNotContain("Wi-Fi password", content);
    }

    private static string GetRepositoryRoot()
    {
        return Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
    }
}
