using Microsoft.EntityFrameworkCore.Design;

namespace OrderFlow.Payments.Infrastructure.Persistence;

public sealed class PaymentsDbContextFactory : IDesignTimeDbContextFactory<PaymentsDbContext>
{
    public PaymentsDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("ConnectionStrings__PaymentsDatabase")
            ?? throw new InvalidOperationException(
                "Set ConnectionStrings__PaymentsDatabase for EF tooling."
            );
        return new PaymentsDbContext(
            new DbContextOptionsBuilder<PaymentsDbContext>()
                .UseNpgsql(connectionString)
                .UseSnakeCaseNamingConvention()
                .Options
        );
    }
}
