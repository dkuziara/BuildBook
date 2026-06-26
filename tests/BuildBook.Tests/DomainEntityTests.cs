using System.Reflection;
using BuildBook.Domain.BuildRecords;
using BuildBook.Domain.Customers;
using BuildBook.Domain.Security;
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
        Assert.IsAssignableFrom<DbSet<ApplicationUser>>(context.ApplicationUsers);
        Assert.IsAssignableFrom<DbSet<ApplicationRole>>(context.ApplicationRoles);
        Assert.IsAssignableFrom<DbSet<ApplicationUserRole>>(context.ApplicationUserRoles);
    }

    [Fact]
    public void BuildRecordModelHasLookupIndexes()
    {
        var options = new DbContextOptionsBuilder<BuildBookDbContext>()
            .UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=BuildBookIndexTest;Trusted_Connection=True;TrustServerCertificate=True")
            .Options;

        using var context = new BuildBookDbContext(options);
        var buildRecord = context.Model.FindEntityType(typeof(BuildRecord));

        Assert.NotNull(buildRecord);

        var indexedProperties = buildRecord.GetIndexes()
            .Select(index => string.Join(",", index.Properties.Select(property => property.Name)))
            .ToArray();

        Assert.Contains(nameof(BuildRecord.SerialNumber), indexedProperties);
        Assert.Contains(nameof(BuildRecord.ProductCode), indexedProperties);
        Assert.Contains(nameof(BuildRecord.ProductName), indexedProperties);
        Assert.Contains(nameof(BuildRecord.CustomerId), indexedProperties);
        Assert.Contains(nameof(BuildRecord.MachineName), indexedProperties);
        Assert.Contains(nameof(BuildRecord.InvoiceNumber), indexedProperties);
        Assert.Contains(nameof(BuildRecord.CustomerOrder), indexedProperties);
        Assert.Contains(nameof(BuildRecord.OANumber), indexedProperties);
        Assert.Contains(nameof(BuildRecord.RadSightVersion), indexedProperties);
        Assert.Contains(nameof(BuildRecord.WindowsVersion), indexedProperties);
        Assert.Contains(nameof(BuildRecord.DateShipped), indexedProperties);
        Assert.Contains(nameof(BuildRecord.LastUpdatedAt), indexedProperties);
    }

    [Fact]
    public void CustomerModelHasNameLookupIndex()
    {
        var options = new DbContextOptionsBuilder<BuildBookDbContext>()
            .UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=BuildBookIndexTest;Trusted_Connection=True;TrustServerCertificate=True")
            .Options;

        using var context = new BuildBookDbContext(options);
        var customer = context.Model.FindEntityType(typeof(Customer));

        Assert.NotNull(customer);

        var indexedProperties = customer.GetIndexes()
            .Select(index => string.Join(",", index.Properties.Select(property => property.Name)))
            .ToArray();

        Assert.Contains(nameof(Customer.Name), indexedProperties);
    }

    [Fact]
    public void BuildRecordSecretModelHasUniqueRecordAndTypeIndex()
    {
        var options = new DbContextOptionsBuilder<BuildBookDbContext>()
            .UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=BuildBookSecretIndexTest;Trusted_Connection=True;TrustServerCertificate=True")
            .Options;

        using var context = new BuildBookDbContext(options);
        var secret = context.Model.FindEntityType(typeof(BuildRecordSecret));

        Assert.NotNull(secret);

        var uniqueIndexes = secret.GetIndexes()
            .Where(index => index.IsUnique)
            .Select(index => string.Join(",", index.Properties.Select(property => property.Name)))
            .ToArray();

        Assert.Contains("BuildRecordId,SecretType", uniqueIndexes);
    }

    [Fact]
    public void ApplicationUserAndRoleModelsHaveUniqueLookupIndexes()
    {
        var options = new DbContextOptionsBuilder<BuildBookDbContext>()
            .UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=BuildBookUserIndexTest;Trusted_Connection=True;TrustServerCertificate=True")
            .Options;

        using var context = new BuildBookDbContext(options);
        var applicationUser = context.Model.FindEntityType(typeof(ApplicationUser));
        var applicationRole = context.Model.FindEntityType(typeof(ApplicationRole));
        var applicationUserRole = context.Model.FindEntityType(typeof(ApplicationUserRole));

        Assert.NotNull(applicationUser);
        Assert.NotNull(applicationRole);
        Assert.NotNull(applicationUserRole);

        Assert.Contains(
            applicationUser.GetIndexes().Where(index => index.IsUnique).Select(index => string.Join(",", index.Properties.Select(property => property.Name))),
            index => index == nameof(ApplicationUser.WindowsUserName));
        Assert.Contains(
            applicationRole.GetIndexes().Where(index => index.IsUnique).Select(index => string.Join(",", index.Properties.Select(property => property.Name))),
            index => index == nameof(ApplicationRole.Name));
        Assert.Equal(
            "ApplicationUserId,ApplicationRoleId",
            string.Join(",", applicationUserRole.FindPrimaryKey()!.Properties.Select(property => property.Name)));
    }
}
