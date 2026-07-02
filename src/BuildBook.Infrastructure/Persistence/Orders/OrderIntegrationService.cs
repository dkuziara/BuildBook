using BuildBook.Application.Orders;
using BuildBook.Domain.Orders;
using BuildBook.Domain.Security;
using Microsoft.EntityFrameworkCore;

namespace BuildBook.Infrastructure.Persistence.Orders;

public sealed class OrderIntegrationService(
    IDbContextFactory<BuildBookDbContext> dbContextFactory) : IOrderIntegrationService
{
    public async Task<OrderOperationResult> UpdateCustomerAndSupportAsync(
        int orderRecordId,
        UpdateOrderCustomerAndSupportRequest request,
        string updatedBy,
        CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var orderRecord = await dbContext.OrderRecords
            .SingleOrDefaultAsync(record => record.Id == orderRecordId && record.IsActive, cancellationToken);

        if (orderRecord is null)
        {
            return OrderOperationResult.Failure("Order was not found.");
        }

        if (request.CustomerId is not null
            && !await dbContext.Customers.AnyAsync(customer => customer.Id == request.CustomerId.Value && customer.IsActive, cancellationToken))
        {
            return OrderOperationResult.Failure("The selected customer could not be found.");
        }

        var productCode = NormalizeOptionalValue(request.ProductCode);
        var customerReference = NormalizeOptionalValue(request.CustomerReference);
        var customerPurchaseOrderNumber = NormalizeOptionalValue(request.CustomerPurchaseOrderNumber);
        var internalOrderReference = NormalizeOptionalValue(request.InternalOrderReference);
        var quoteNumber = NormalizeOptionalValue(request.QuoteNumber);
        var supportTicketNo = NormalizeOptionalValue(request.SupportTicketNo);

        if (orderRecord.ProductCode == productCode
            && orderRecord.CustomerId == request.CustomerId
            && orderRecord.CustomerReference == customerReference
            && orderRecord.CustomerPurchaseOrderNumber == customerPurchaseOrderNumber
            && orderRecord.InternalOrderReference == internalOrderReference
            && orderRecord.QuoteNumber == quoteNumber
            && orderRecord.SupportTicketNo == supportTicketNo)
        {
            return OrderOperationResult.Success();
        }

        orderRecord.ProductCode = productCode;
        orderRecord.CustomerId = request.CustomerId;
        orderRecord.CustomerReference = customerReference;
        orderRecord.CustomerPurchaseOrderNumber = customerPurchaseOrderNumber;
        orderRecord.InternalOrderReference = internalOrderReference;
        orderRecord.QuoteNumber = quoteNumber;
        orderRecord.SupportTicketNo = supportTicketNo;

        await SaveOrderAsync(dbContext, orderRecord, updatedBy, cancellationToken);
        return OrderOperationResult.Success();
    }

    public async Task<OrderOperationResult> LinkBuildRecordAsync(
        int orderRecordId,
        int buildRecordId,
        string updatedBy,
        CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var orderRecord = await dbContext.OrderRecords
            .Include(record => record.BuildRecordLinks)
            .SingleOrDefaultAsync(record => record.Id == orderRecordId && record.IsActive, cancellationToken);

        if (orderRecord is null)
        {
            return OrderOperationResult.Failure("Order was not found.");
        }

        var buildRecordExists = await dbContext.BuildRecords
            .AnyAsync(buildRecord => buildRecord.Id == buildRecordId && buildRecord.IsActive, cancellationToken);

        if (!buildRecordExists)
        {
            return OrderOperationResult.Failure("Build Record was not found.");
        }

        if (orderRecord.BuildRecordLinks.Any(link => link.BuildRecordId == buildRecordId))
        {
            return OrderOperationResult.Success();
        }

        var actor = NormalizeActor(updatedBy);
        var actorUser = await FindApplicationUserAsync(dbContext, actor, cancellationToken);

        orderRecord.BuildRecordLinks.Add(new OrderBuildRecordLink
        {
            BuildRecordId = buildRecordId,
            LinkType = "Order",
            LinkedAt = DateTimeOffset.UtcNow,
            LinkedByUserId = actorUser?.Id
        });

        await SaveOrderAsync(dbContext, orderRecord, actor, cancellationToken, actorUser);
        return OrderOperationResult.Success();
    }

    public async Task<OrderOperationResult> UnlinkBuildRecordAsync(
        int orderRecordId,
        int buildRecordId,
        string updatedBy,
        CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var orderRecord = await dbContext.OrderRecords
            .SingleOrDefaultAsync(record => record.Id == orderRecordId && record.IsActive, cancellationToken);

        if (orderRecord is null)
        {
            return OrderOperationResult.Failure("Order was not found.");
        }

        var link = await dbContext.OrderBuildRecordLinks
            .SingleOrDefaultAsync(
                orderLink => orderLink.OrderRecordId == orderRecordId && orderLink.BuildRecordId == buildRecordId,
                cancellationToken);

        if (link is null)
        {
            return OrderOperationResult.Failure("Build Record link was not found.");
        }

        dbContext.OrderBuildRecordLinks.Remove(link);
        await SaveOrderAsync(dbContext, orderRecord, updatedBy, cancellationToken);
        return OrderOperationResult.Success();
    }

    private static async Task SaveOrderAsync(
        BuildBookDbContext dbContext,
        OrderRecord orderRecord,
        string updatedBy,
        CancellationToken cancellationToken,
        ApplicationUser? actorUser = null)
    {
        orderRecord.LastUpdatedAt = DateTimeOffset.UtcNow;
        orderRecord.LastUpdatedByUserId = actorUser?.Id;
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static async Task<ApplicationUser?> FindApplicationUserAsync(
        BuildBookDbContext dbContext,
        string actor,
        CancellationToken cancellationToken)
    {
        return await dbContext.ApplicationUsers
            .SingleOrDefaultAsync(
                user => user.IsActive
                    && (user.WindowsUserName == actor
                        || user.EmailAddress == actor
                        || user.DisplayName == actor),
                cancellationToken);
    }

    private static string NormalizeActor(string? actor)
    {
        return string.IsNullOrWhiteSpace(actor) ? "Unknown" : actor.Trim();
    }

    private static string? NormalizeOptionalValue(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
