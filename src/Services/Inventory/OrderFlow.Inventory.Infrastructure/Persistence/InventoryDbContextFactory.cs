using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace OrderFlow.Inventory.Infrastructure.Persistence;

public sealed class InventoryDbContextFactory : IDesignTimeDbContextFactory<InventoryDbContext>
{
    public InventoryDbContext CreateDbContext(string[] args) =>
        new(
            new DbContextOptionsBuilder<InventoryDbContext>()
                .UseNpgsql(
                    "Host=localhost;Database=orderflow_inventory;Username=postgres;Password=postgres"
                )
                .UseSnakeCaseNamingConvention()
                .Options
        );
}
