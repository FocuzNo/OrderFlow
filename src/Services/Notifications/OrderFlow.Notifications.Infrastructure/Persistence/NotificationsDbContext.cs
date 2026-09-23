using Microsoft.EntityFrameworkCore;
using OrderFlow.Notifications.Application.Abstractions.Persistence;
using OrderFlow.Notifications.Domain.Notifications;

namespace OrderFlow.Notifications.Infrastructure.Persistence;

public sealed class NotificationsDbContext(DbContextOptions<NotificationsDbContext> options)
    : DbContext(options),
        IUnitOfWork
{
    public DbSet<Notification> Notifications => Set<Notification>();

    public DbSet<NotificationDeliveryAttempt> Attempts => Set<NotificationDeliveryAttempt>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(NotificationsDbContext).Assembly);
}
