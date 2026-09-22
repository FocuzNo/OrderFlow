using Microsoft.EntityFrameworkCore.Design;

namespace OrderFlow.Inventory.Infrastructure.Persistence;

public sealed class InventoryDbContextFactory : IDesignTimeDbContextFactory<InventoryDbContext>
{
    public InventoryDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("ConnectionStrings__InventoryDatabase")
            ?? throw new InvalidOperationException(
                "Set ConnectionStrings__InventoryDatabase for EF tooling."
            );
        return new InventoryDbContext(
            new DbContextOptionsBuilder<InventoryDbContext>()
                .UseNpgsql(connectionString)
                .UseSnakeCaseNamingConvention()
                .Options
        );
    }
}
