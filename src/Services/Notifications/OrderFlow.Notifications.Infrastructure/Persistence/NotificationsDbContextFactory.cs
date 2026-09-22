using Microsoft.EntityFrameworkCore.Design;

namespace OrderFlow.Notifications.Infrastructure.Persistence;

public sealed class NotificationsDbContextFactory
    : IDesignTimeDbContextFactory<NotificationsDbContext>
{
    public NotificationsDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("ConnectionStrings__NotificationsDatabase")
            ?? throw new InvalidOperationException(
                "Set ConnectionStrings__NotificationsDatabase for EF tooling."
            );
        return new NotificationsDbContext(
            new DbContextOptionsBuilder<NotificationsDbContext>()
                .UseNpgsql(connectionString)
                .UseSnakeCaseNamingConvention()
                .Options
        );
    }
}
