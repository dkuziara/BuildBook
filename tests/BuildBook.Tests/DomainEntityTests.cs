using System.Reflection;
using BuildBook.Domain.BuildRecords;
using BuildBook.Domain.Customers;
using BuildBook.Domain.Orders;
using BuildBook.Domain.Rmas;
using BuildBook.Domain.Security;
using BuildBook.Domain.Settings;
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
        Assert.IsAssignableFrom<DbSet<CustomerContractDocument>>(context.CustomerContractDocuments);
        Assert.IsAssignableFrom<DbSet<SupportContractLevel>>(context.SupportContractLevels);
        Assert.IsAssignableFrom<DbSet<BuildRecordSecret>>(context.BuildRecordSecrets);
        Assert.IsAssignableFrom<DbSet<BuildRecordAudit>>(context.BuildRecordAudit);
        Assert.IsAssignableFrom<DbSet<ImportBatch>>(context.Imports);
        Assert.IsAssignableFrom<DbSet<ApplicationUser>>(context.ApplicationUsers);
        Assert.IsAssignableFrom<DbSet<ApplicationRole>>(context.ApplicationRoles);
        Assert.IsAssignableFrom<DbSet<ApplicationUserRole>>(context.ApplicationUserRoles);
        Assert.IsAssignableFrom<DbSet<SystemSetting>>(context.SystemSettings);
        Assert.IsAssignableFrom<DbSet<OrderRecord>>(context.OrderRecords);
        Assert.IsAssignableFrom<DbSet<OrderAssignment>>(context.OrderAssignments);
        Assert.IsAssignableFrom<DbSet<OrderChecklistItem>>(context.OrderChecklistItems);
        Assert.IsAssignableFrom<DbSet<OrderNote>>(context.OrderNotes);
        Assert.IsAssignableFrom<DbSet<OrderLabel>>(context.OrderLabels);
        Assert.IsAssignableFrom<DbSet<OrderBuildRecordLink>>(context.OrderBuildRecordLinks);
        Assert.IsAssignableFrom<DbSet<OrderStatusHistory>>(context.OrderStatusHistory);
        Assert.IsAssignableFrom<DbSet<OrderImportBatch>>(context.OrderImportBatches);
        Assert.IsAssignableFrom<DbSet<OrderImportWarning>>(context.OrderImportWarnings);
        Assert.IsAssignableFrom<DbSet<RmaRecord>>(context.RmaRecords);
        Assert.IsAssignableFrom<DbSet<RmaChecklistItem>>(context.RmaChecklistItems);
        Assert.IsAssignableFrom<DbSet<RmaNote>>(context.RmaNotes);
        Assert.IsAssignableFrom<DbSet<RmaCommunication>>(context.RmaCommunications);
        Assert.IsAssignableFrom<DbSet<RmaAttachment>>(context.RmaAttachments);
        Assert.IsAssignableFrom<DbSet<RmaPart>>(context.RmaParts);
        Assert.IsAssignableFrom<DbSet<RmaStatusHistory>>(context.RmaStatusHistory);
        Assert.IsAssignableFrom<DbSet<RmaAudit>>(context.RmaAudit);
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
        Assert.Contains(nameof(Customer.SupportContractLevelId), indexedProperties);
        Assert.Contains(nameof(Customer.SupportContractStatus), indexedProperties);
        Assert.Contains(nameof(Customer.IsActive), indexedProperties);
    }

    [Fact]
    public void CustomerSupportContractsModelIncludesExpectedFields()
    {
        var propertyNames = typeof(Customer)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Select(property => property.Name)
            .ToArray();

        Assert.Contains(nameof(Customer.AccountCode), propertyNames);
        Assert.Contains(nameof(Customer.PrimaryContactName), propertyNames);
        Assert.Contains(nameof(Customer.SupportContractLevelId), propertyNames);
        Assert.Contains(nameof(Customer.SupportContractStatus), propertyNames);
        Assert.Contains(nameof(Customer.SupportContractStartDate), propertyNames);
        Assert.Contains(nameof(Customer.SupportContractEndDate), propertyNames);
        Assert.Contains(nameof(Customer.SupportNotes), propertyNames);
        Assert.Contains(nameof(Customer.ContractDocuments), propertyNames);
        Assert.Contains(nameof(Customer.OrderRecords), propertyNames);
    }

    [Fact]
    public void SupportContractLevelsAndSystemSettingsModelsHaveLookupIndexes()
    {
        var options = new DbContextOptionsBuilder<BuildBookDbContext>()
            .UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=BuildBookContractIndexTest;Trusted_Connection=True;TrustServerCertificate=True")
            .Options;

        using var context = new BuildBookDbContext(options);
        var supportContractLevel = context.Model.FindEntityType(typeof(SupportContractLevel));
        var systemSetting = context.Model.FindEntityType(typeof(SystemSetting));

        Assert.NotNull(supportContractLevel);
        Assert.NotNull(systemSetting);

        Assert.Contains(
            supportContractLevel.GetIndexes().Where(index => index.IsUnique).Select(index => string.Join(",", index.Properties.Select(property => property.Name))),
            index => index == nameof(SupportContractLevel.Name));
        Assert.Contains(
            systemSetting.GetIndexes().Where(index => index.IsUnique).Select(index => string.Join(",", index.Properties.Select(property => property.Name))),
            index => index == nameof(SystemSetting.Key));
    }

    [Fact]
    public void CustomerSupportContractStatusAndResponseTimeUnitsCoverSpecificationValues()
    {
        Assert.Equal(
            [
                "No Contract",
                "Active",
                "Expired",
                "Pending Renewal",
                "Suspended",
                "Unknown"
            ],
            CustomerSupportContractStatuses.All);

        Assert.Contains(nameof(SupportResponseTimeUnit.Hours), Enum.GetNames<SupportResponseTimeUnit>());
        Assert.Contains(nameof(SupportResponseTimeUnit.WorkingHours), Enum.GetNames<SupportResponseTimeUnit>());
        Assert.Contains(nameof(SupportResponseTimeUnit.Days), Enum.GetNames<SupportResponseTimeUnit>());
        Assert.Contains(nameof(SupportResponseTimeUnit.WorkingDays), Enum.GetNames<SupportResponseTimeUnit>());
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

    [Fact]
    public void OrderEnumsCoverFoundationSpecificationValues()
    {
        Assert.Contains(nameof(OrderPriority.Urgent), Enum.GetNames<OrderPriority>());
        Assert.Contains(nameof(OrderAssignmentType.Owner), Enum.GetNames<OrderAssignmentType>());
        Assert.Contains(nameof(OrderAssignmentType.SalesAdmin), Enum.GetNames<OrderAssignmentType>());
        Assert.Contains(nameof(OrderAssignmentType.Qa), Enum.GetNames<OrderAssignmentType>());
        Assert.Contains(nameof(OrderNoteType.PlannerImportedNote), Enum.GetNames<OrderNoteType>());
        Assert.Contains(nameof(OrderImportWarningSeverity.Warning), Enum.GetNames<OrderImportWarningSeverity>());
        Assert.Contains(nameof(OrderImportWarningSeverity.Error), Enum.GetNames<OrderImportWarningSeverity>());
    }

    [Fact]
    public void OrderRecordModelIncludesExpectedWorkflowAndImportFields()
    {
        var propertyNames = typeof(OrderRecord)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Select(property => property.Name)
            .ToArray();

        Assert.Contains(nameof(OrderRecord.OrderNumber), propertyNames);
        Assert.Contains(nameof(OrderRecord.OrderTitle), propertyNames);
        Assert.Contains(nameof(OrderRecord.Status), propertyNames);
        Assert.Contains(nameof(OrderRecord.Priority), propertyNames);
        Assert.Contains(nameof(OrderRecord.PlannerTaskId), propertyNames);
        Assert.Contains(nameof(OrderRecord.PlannerPlanId), propertyNames);
        Assert.Contains(nameof(OrderRecord.PlannerBucketId), propertyNames);
        Assert.Contains(nameof(OrderRecord.PlannerSource), propertyNames);
        Assert.Contains(nameof(OrderRecord.ImportedPriorityText), propertyNames);
        Assert.Contains(nameof(OrderRecord.ImportedCreatedByText), propertyNames);
        Assert.Contains(nameof(OrderRecord.ImportedCompletedByText), propertyNames);
        Assert.Contains(nameof(OrderRecord.BuildRecordLinks), propertyNames);
    }

    [Fact]
    public void OrderRecordModelHasOperationalIndexes()
    {
        var options = new DbContextOptionsBuilder<BuildBookDbContext>()
            .UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=BuildBookOrderIndexTest;Trusted_Connection=True;TrustServerCertificate=True")
            .Options;

        using var context = new BuildBookDbContext(options);
        var orderRecord = context.Model.FindEntityType(typeof(OrderRecord));

        Assert.NotNull(orderRecord);

        var indexedProperties = orderRecord.GetIndexes()
            .Select(index => string.Join(",", index.Properties.Select(property => property.Name)))
            .ToArray();

        Assert.Contains(nameof(OrderRecord.OrderNumber), indexedProperties);
        Assert.Contains(nameof(OrderRecord.CustomerId), indexedProperties);
        Assert.Contains(nameof(OrderRecord.Status), indexedProperties);
        Assert.Contains(nameof(OrderRecord.Priority), indexedProperties);
        Assert.Contains(nameof(OrderRecord.StartDate), indexedProperties);
        Assert.Contains(nameof(OrderRecord.DueDate), indexedProperties);
        Assert.Contains(nameof(OrderRecord.PlannerTaskId), indexedProperties);
        Assert.Contains(nameof(OrderRecord.InvoiceNumber), indexedProperties);
        Assert.Contains(nameof(OrderRecord.SupportTicketNo), indexedProperties);
        Assert.Contains(nameof(OrderRecord.LastUpdatedAt), indexedProperties);
    }

    [Fact]
    public void BuildRecordModelIncludesOrderLinkNavigation()
    {
        var propertyNames = typeof(BuildRecord)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Select(property => property.Name)
            .ToArray();

        Assert.Contains(nameof(BuildRecord.OrderLinks), propertyNames);
    }

    [Fact]
    public void RmaEnumsCoverFoundationSpecificationValues()
    {
        Assert.Contains(nameof(RmaStatus.BookedIn), Enum.GetNames<RmaStatus>());
        Assert.Contains(nameof(RmaStatus.WorkInProgress), Enum.GetNames<RmaStatus>());
        Assert.Contains(nameof(RmaStatus.ReadyToShip), Enum.GetNames<RmaStatus>());
        Assert.Contains(nameof(RmaPriority.Urgent), Enum.GetNames<RmaPriority>());
        Assert.Contains(nameof(RmaWarrantyStatus.WarrantyUnknown), Enum.GetNames<RmaWarrantyStatus>());
        Assert.Contains(nameof(RmaFaultCategory.DiskStorageIssue), Enum.GetNames<RmaFaultCategory>());
        Assert.Contains(nameof(RmaRootCauseCategory.DiskStorageFailure), Enum.GetNames<RmaRootCauseCategory>());
        Assert.Contains(nameof(RmaTestResult.NotTested), Enum.GetNames<RmaTestResult>());
        Assert.Contains(nameof(RmaQaResult.NotRequired), Enum.GetNames<RmaQaResult>());
        Assert.Contains(nameof(RmaOutcome.RepairedAndReturned), Enum.GetNames<RmaOutcome>());
        Assert.Contains(nameof(RmaNoteType.CommercialNote), Enum.GetNames<RmaNoteType>());
    }

    [Fact]
    public void RmaRecordModelHasOperationalIndexes()
    {
        var options = new DbContextOptionsBuilder<BuildBookDbContext>()
            .UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=BuildBookRmaIndexTest;Trusted_Connection=True;TrustServerCertificate=True")
            .Options;

        using var context = new BuildBookDbContext(options);
        var rmaRecord = context.Model.FindEntityType(typeof(RmaRecord));

        Assert.NotNull(rmaRecord);

        var indexedProperties = rmaRecord.GetIndexes()
            .Select(index => string.Join(",", index.Properties.Select(property => property.Name)))
            .ToArray();

        Assert.Contains(nameof(RmaRecord.RmaNumber), indexedProperties);
        Assert.Contains(nameof(RmaRecord.BuildRecordId), indexedProperties);
        Assert.Contains(nameof(RmaRecord.CustomerId), indexedProperties);
        Assert.Contains(nameof(RmaRecord.Status), indexedProperties);
        Assert.Contains(nameof(RmaRecord.AssignedTo), indexedProperties);
        Assert.Contains(nameof(RmaRecord.Priority), indexedProperties);
        Assert.Contains(nameof(RmaRecord.DueDate), indexedProperties);
        Assert.Contains(nameof(RmaRecord.SerialNumber), indexedProperties);
        Assert.Contains(nameof(RmaRecord.ProductCode), indexedProperties);
        Assert.Contains(nameof(RmaRecord.ProductName), indexedProperties);
        Assert.Contains(nameof(RmaRecord.LastUpdatedAt), indexedProperties);
    }
}
