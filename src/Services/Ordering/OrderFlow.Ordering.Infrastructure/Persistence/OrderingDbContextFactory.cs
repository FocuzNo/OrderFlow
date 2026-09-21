using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace OrderFlow.Ordering.Infrastructure.Persistence;

public sealed class OrderingDbContextFactory : IDesignTimeDbContextFactory<OrderingDbContext>
{
    public OrderingDbContext CreateDbContext(string[] a) =>
        new(
            new DbContextOptionsBuilder<OrderingDbContext>()
                .UseNpgsql(
                    "Host=localhost;Database=orderflow_ordering;Username=postgres;Password=postgres"
                )
                .UseSnakeCaseNamingConvention()
                .Options
        );
}
