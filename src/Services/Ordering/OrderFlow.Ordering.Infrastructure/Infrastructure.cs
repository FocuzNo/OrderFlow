using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderFlow.Ordering.Application;
using OrderFlow.Ordering.Domain;

namespace OrderFlow.Ordering.Infrastructure;

public sealed class OutboxMessage { public Guid Id { get; set; } public string Type { get; set; } = string.Empty; public string Payload { get; set; } = string.Empty; public DateTimeOffset OccurredAt { get; set; } public DateTimeOffset? ProcessedAt { get; set; } public string? Error { get; set; } }
public sealed class OrderingDbContext(DbContextOptions<OrderingDbContext> options) : DbContext(options)
{
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        var aggregates = ChangeTracker.Entries<IHasDomainEvents>().Select(x => x.Entity).Where(x => x.DomainEvents.Count > 0).ToArray();
        foreach (var aggregate in aggregates) { foreach (var item in aggregate.DomainEvents) OutboxMessages.Add(new OutboxMessage { Id = item.Id, Type = item.Type, Payload = JsonSerializer.Serialize(item), OccurredAt = item.OccurredAt }); aggregate.ClearDomainEvents(); }
        return await base.SaveChangesAsync(ct);
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>(b => { b.ToTable("orders"); b.HasKey(x => x.Id); b.Property(x => x.Reference).HasMaxLength(Order.MaxReferenceLength).IsRequired(); b.Property(x => x.Description).HasMaxLength(Order.MaxDescriptionLength); b.Property(x => x.Value).HasPrecision(18, 2); b.Property(x => x.Status).HasMaxLength(50).IsRequired(); b.Ignore(x => x.DomainEvents); });
        modelBuilder.Entity<OutboxMessage>(b => { b.ToTable("outbox_messages"); b.HasKey(x => x.Id); b.Property(x => x.Type).HasMaxLength(250).IsRequired(); b.Property(x => x.Payload).HasColumnType("jsonb").IsRequired(); b.Property(x => x.Error).HasMaxLength(2000); b.HasIndex(x => new { x.ProcessedAt, x.OccurredAt }); });
    }
}
public sealed class OrderRepository(OrderingDbContext db) : IOrderRepository
{
    public Task<Order?> GetAsync(Guid id, CancellationToken ct) => db.Orders.SingleOrDefaultAsync(x => x.Id == id, ct);
    public async Task<IReadOnlyList<Order>> ListAsync(CancellationToken ct) => await db.Orders.AsNoTracking().OrderBy(x => x.CreatedAt).ToListAsync(ct);
    public async Task AddAsync(Order entity, CancellationToken ct) { db.Orders.Add(entity); await db.SaveChangesAsync(ct); }
    public async Task SaveAsync(CancellationToken ct) => await db.SaveChangesAsync(ct);
    public async Task DeleteAsync(Order entity, CancellationToken ct) { db.Orders.Remove(entity); await db.SaveChangesAsync(ct); }
}
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration) { var connection = configuration.GetConnectionString("OrderingDatabase") ?? throw new InvalidOperationException("ConnectionStrings:OrderingDatabase is required."); services.AddDbContext<OrderingDbContext>(o => o.UseNpgsql(connection)); services.AddScoped<IOrderRepository, OrderRepository>(); return services; }
}
public sealed class DesignTimeFactory : IDesignTimeDbContextFactory<OrderingDbContext>
{
    public OrderingDbContext CreateDbContext(string[] args) { var builder = new DbContextOptionsBuilder<OrderingDbContext>(); builder.UseNpgsql("Host=localhost;Database=orderflow_ordering;Username=postgres;Password=postgres"); return new OrderingDbContext(builder.Options); }
}
