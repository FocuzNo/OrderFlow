using Microsoft.EntityFrameworkCore.Design;

namespace OrderFlow.Notifications.Infrastructure.Persistence;

public sealed class NotificationsDbContextFactory
    : IDesignTimeDbContextFactory<NotificationsDbContext>
{
    public NotificationsDbContext CreateDbContext(string[] a) =>
        new(
            new DbContextOptionsBuilder<NotificationsDbContext>()
                .UseNpgsql(
                    "Host=localhost;Database=orderflow_notifications;Username=postgres;Password=postgres"
                )
                .UseSnakeCaseNamingConvention()
                .Options
        );
}
