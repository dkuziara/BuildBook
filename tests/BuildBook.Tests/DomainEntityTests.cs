using System.Reflection;
using BuildBook.Domain.BuildRecords;
using BuildBook.Domain.Customers;
using BuildBook.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BuildBook.Tests;

public class DomainEntityTests
{
    [Fact]
    public void BuildRecordDoesNotStoreSensitiveValues()
    {
        var propertyNames = typeof(BuildRecord)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Select(property => property.Name)
            .ToArray();

        Assert.DoesNotContain("RadSightUserPassword", propertyNames);
        Assert.DoesNotContain("WindowsAdminPassword", propertyNames);
        Assert.DoesNotContain("KioskPassword", propertyNames);
        Assert.DoesNotContain("WifiPassword", propertyNames);
        Assert.DoesNotContain("RouterPassword", propertyNames);
        Assert.DoesNotContain("BitLockerRecoveryKey", propertyNames);
    }

    [Fact]
    public void SecretTypesCoverSensitiveFieldsFromSpecification()
    {
        var secretTypes = Enum.GetNames<SecretType>();

        Assert.Contains(nameof(SecretType.RadSightUserPassword), secretTypes);
        Assert.Contains(nameof(SecretType.WindowsAdminPassword), secretTypes);
        Assert.Contains(nameof(SecretType.KioskPassword), secretTypes);
        Assert.Contains(nameof(SecretType.WifiPassword), secretTypes);
        Assert.Contains(nameof(SecretType.RouterPassword), secretTypes);
        Assert.Contains(nameof(SecretType.BitLockerRecoveryKey), secretTypes);
    }

    [Fact]
    public void DbContextExposesCoreEntitySets()
    {
        var options = new DbContextOptionsBuilder<BuildBookDbContext>()
            .UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=BuildBookModelTest;Trusted_Connection=True;TrustServerCertificate=True")
            .Options;

        using var context = new BuildBookDbContext(options);

        Assert.IsAssignableFrom<DbSet<BuildRecord>>(context.BuildRecords);
        Assert.IsAssignableFrom<DbSet<Customer>>(context.Customers);
        Assert.IsAssignableFrom<DbSet<BuildRecordSecret>>(context.BuildRecordSecrets);
        Assert.IsAssignableFrom<DbSet<BuildRecordAudit>>(context.BuildRecordAudit);
        Assert.IsAssignableFrom<DbSet<ImportBatch>>(context.Imports);
    }
}
