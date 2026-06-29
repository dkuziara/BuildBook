using BuildBook.Application.Rmas;
using Microsoft.EntityFrameworkCore;

namespace BuildBook.Infrastructure.Persistence.Rmas;

public sealed class RmaNumberGenerator(
    IDbContextFactory<BuildBookDbContext> dbContextFactory) : IRmaNumberGenerator
{
    public async Task<string> GenerateNextAsync(CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        return await GenerateNextAsync(dbContext, cancellationToken);
    }

    internal static async Task<string> GenerateNextAsync(
        BuildBookDbContext dbContext,
        CancellationToken cancellationToken = default)
    {
        var rmaNumbers = await dbContext.RmaRecords
            .AsNoTracking()
            .Select(rmaRecord => rmaRecord.RmaNumber)
            .ToListAsync(cancellationToken);

        var highestValue = rmaNumbers
            .Select(ParseRmaNumber)
            .Where(value => value.HasValue)
            .Max();

        var nextNumber = (highestValue ?? 0) + 1;
        return $"RMA-{nextNumber:0000}";
    }

    private static int? ParseRmaNumber(string? rmaNumber)
    {
        if (string.IsNullOrWhiteSpace(rmaNumber))
        {
            return null;
        }

        var lastHyphenIndex = rmaNumber.LastIndexOf("-", StringComparison.Ordinal);
        var numericPart = lastHyphenIndex >= 0
            ? rmaNumber[(lastHyphenIndex + 1)..]
            : rmaNumber;

        return int.TryParse(numericPart, out var parsedValue)
            ? parsedValue
            : null;
    }
}
