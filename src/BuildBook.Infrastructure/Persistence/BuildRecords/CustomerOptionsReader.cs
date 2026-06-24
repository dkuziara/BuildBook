using BuildBook.Application.BuildRecords;
using Microsoft.EntityFrameworkCore;

namespace BuildBook.Infrastructure.Persistence.BuildRecords;

public sealed class CustomerOptionsReader(
    IDbContextFactory<BuildBookDbContext> dbContextFactory) : ICustomerOptionsReader
{
    public async Task<IReadOnlyList<CustomerOption>> ListActiveAsync(
        CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        return await dbContext.Customers
            .AsNoTracking()
            .Where(customer => customer.IsActive)
            .OrderBy(customer => customer.Name)
            .Select(customer => new CustomerOption(customer.Id, customer.Name))
            .ToListAsync(cancellationToken);
    }
}
