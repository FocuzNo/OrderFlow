using Microsoft.EntityFrameworkCore.Design;

namespace OrderFlow.Catalog.Infrastructure.Persistence;

public sealed class CatalogDbContextFactory : IDesignTimeDbContextFactory<CatalogDbContext>
{
    public CatalogDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("ConnectionStrings__CatalogDatabase")
            ?? throw new InvalidOperationException(
                "Set ConnectionStrings__CatalogDatabase for EF tooling."
            );
        return new CatalogDbContext(
            new DbContextOptionsBuilder<CatalogDbContext>()
                .UseNpgsql(connectionString)
                .UseSnakeCaseNamingConvention()
                .Options
        );
    }
}
