using Microsoft.EntityFrameworkCore;

namespace BuildBook.Infrastructure.Persistence;

public sealed class BuildBookDbContext(DbContextOptions<BuildBookDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BuildBookDbContext).Assembly);
    }
}
