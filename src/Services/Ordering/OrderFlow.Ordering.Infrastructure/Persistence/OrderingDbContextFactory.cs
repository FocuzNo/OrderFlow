using Microsoft.EntityFrameworkCore.Design;

namespace OrderFlow.Ordering.Infrastructure.Persistence;

public sealed class OrderingDbContextFactory : IDesignTimeDbContextFactory<OrderingDbContext>
{
    public OrderingDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("ConnectionStrings__OrderingDatabase")
            ?? throw new InvalidOperationException(
                "Set ConnectionStrings__OrderingDatabase for EF tooling."
            );
        return new OrderingDbContext(
            new DbContextOptionsBuilder<OrderingDbContext>()
                .UseNpgsql(connectionString)
                .UseSnakeCaseNamingConvention()
                .Options
        );
    }
}
