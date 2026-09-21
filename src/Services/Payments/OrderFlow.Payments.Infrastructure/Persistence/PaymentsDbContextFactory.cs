using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace OrderFlow.Payments.Infrastructure.Persistence;

public sealed class PaymentsDbContextFactory : IDesignTimeDbContextFactory<PaymentsDbContext>
{
    public PaymentsDbContext CreateDbContext(string[] a) =>
        new(
            new DbContextOptionsBuilder<PaymentsDbContext>()
                .UseNpgsql(
                    "Host=localhost;Database=orderflow_payments;Username=postgres;Password=postgres"
                )
                .UseSnakeCaseNamingConvention()
                .Options
        );
}
