using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderFlow.Notifications.Application;
using OrderFlow.Notifications.Domain;

namespace OrderFlow.Notifications.Infrastructure;

public sealed class OutboxMessage { public Guid Id { get; set; } public string Type { get; set; } = string.Empty; public string Payload { get; set; } = string.Empty; public DateTimeOffset OccurredAt { get; set; } public DateTimeOffset? ProcessedAt { get; set; } public string? Error { get; set; } }
public sealed class NotificationsDbContext(DbContextOptions<NotificationsDbContext> options) : DbContext(options)
{
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        var aggregates = ChangeTracker.Entries<IHasDomainEvents>().Select(x => x.Entity).Where(x => x.DomainEvents.Count > 0).ToArray();
        foreach (var aggregate in aggregates) { foreach (var item in aggregate.DomainEvents) OutboxMessages.Add(new OutboxMessage { Id = item.Id, Type = item.Type, Payload = JsonSerializer.Serialize(item), OccurredAt = item.OccurredAt }); aggregate.ClearDomainEvents(); }
        return await base.SaveChangesAsync(ct);
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Notification>(b => { b.ToTable("notifications"); b.HasKey(x => x.Id); b.Property(x => x.Reference).HasMaxLength(Notification.MaxReferenceLength).IsRequired(); b.Property(x => x.Description).HasMaxLength(Notification.MaxDescriptionLength); b.Property(x => x.Value).HasPrecision(18, 2); b.Property(x => x.Status).HasMaxLength(50).IsRequired(); b.Ignore(x => x.DomainEvents); });
        modelBuilder.Entity<OutboxMessage>(b => { b.ToTable("outbox_messages"); b.HasKey(x => x.Id); b.Property(x => x.Type).HasMaxLength(250).IsRequired(); b.Property(x => x.Payload).HasColumnType("jsonb").IsRequired(); b.Property(x => x.Error).HasMaxLength(2000); b.HasIndex(x => new { x.ProcessedAt, x.OccurredAt }); });
    }
}
public sealed class NotificationRepository(NotificationsDbContext db) : INotificationRepository
{
    public Task<Notification?> GetAsync(Guid id, CancellationToken ct) => db.Notifications.SingleOrDefaultAsync(x => x.Id == id, ct);
    public async Task<IReadOnlyList<Notification>> ListAsync(CancellationToken ct) => await db.Notifications.AsNoTracking().OrderBy(x => x.CreatedAt).ToListAsync(ct);
    public async Task AddAsync(Notification entity, CancellationToken ct) { db.Notifications.Add(entity); await db.SaveChangesAsync(ct); }
    public async Task SaveAsync(CancellationToken ct) => await db.SaveChangesAsync(ct);
    public async Task DeleteAsync(Notification entity, CancellationToken ct) { db.Notifications.Remove(entity); await db.SaveChangesAsync(ct); }
}
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration) { var connection = configuration.GetConnectionString("NotificationsDatabase") ?? throw new InvalidOperationException("ConnectionStrings:NotificationsDatabase is required."); services.AddDbContext<NotificationsDbContext>(o => o.UseNpgsql(connection)); services.AddScoped<INotificationRepository, NotificationRepository>(); return services; }
}
public sealed class DesignTimeFactory : IDesignTimeDbContextFactory<NotificationsDbContext>
{
    public NotificationsDbContext CreateDbContext(string[] args) { var builder = new DbContextOptionsBuilder<NotificationsDbContext>(); builder.UseNpgsql("Host=localhost;Database=orderflow_notifications;Username=postgres;Password=postgres"); return new NotificationsDbContext(builder.Options); }
}
