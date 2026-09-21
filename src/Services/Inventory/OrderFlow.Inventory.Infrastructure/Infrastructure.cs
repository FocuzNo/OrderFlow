using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderFlow.Inventory.Application;
using OrderFlow.Inventory.Domain;

namespace OrderFlow.Inventory.Infrastructure;

public sealed class OutboxMessage { public Guid Id { get; set; } public string Type { get; set; } = string.Empty; public string Payload { get; set; } = string.Empty; public DateTimeOffset OccurredAt { get; set; } public DateTimeOffset? ProcessedAt { get; set; } public string? Error { get; set; } }
public sealed class InventoryDbContext(DbContextOptions<InventoryDbContext> options) : DbContext(options)
{
    public DbSet<StockItem> StockItems => Set<StockItem>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        var aggregates = ChangeTracker.Entries<IHasDomainEvents>().Select(x => x.Entity).Where(x => x.DomainEvents.Count > 0).ToArray();
        foreach (var aggregate in aggregates) { foreach (var item in aggregate.DomainEvents) OutboxMessages.Add(new OutboxMessage { Id = item.Id, Type = item.Type, Payload = JsonSerializer.Serialize(item), OccurredAt = item.OccurredAt }); aggregate.ClearDomainEvents(); }
        return await base.SaveChangesAsync(ct);
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<StockItem>(b => { b.ToTable("stock_items"); b.HasKey(x => x.Id); b.Property(x => x.Reference).HasMaxLength(StockItem.MaxReferenceLength).IsRequired(); b.Property(x => x.Description).HasMaxLength(StockItem.MaxDescriptionLength); b.Property(x => x.Value).HasPrecision(18, 2); b.Property(x => x.Status).HasMaxLength(50).IsRequired(); b.Ignore(x => x.DomainEvents); });
        modelBuilder.Entity<OutboxMessage>(b => { b.ToTable("outbox_messages"); b.HasKey(x => x.Id); b.Property(x => x.Type).HasMaxLength(250).IsRequired(); b.Property(x => x.Payload).HasColumnType("jsonb").IsRequired(); b.Property(x => x.Error).HasMaxLength(2000); b.HasIndex(x => new { x.ProcessedAt, x.OccurredAt }); });
    }
}
public sealed class StockItemRepository(InventoryDbContext db) : IStockItemRepository
{
    public Task<StockItem?> GetAsync(Guid id, CancellationToken ct) => db.StockItems.SingleOrDefaultAsync(x => x.Id == id, ct);
    public async Task<IReadOnlyList<StockItem>> ListAsync(CancellationToken ct) => await db.StockItems.AsNoTracking().OrderBy(x => x.CreatedAt).ToListAsync(ct);
    public async Task AddAsync(StockItem entity, CancellationToken ct) { db.StockItems.Add(entity); await db.SaveChangesAsync(ct); }
    public async Task SaveAsync(CancellationToken ct) => await db.SaveChangesAsync(ct);
    public async Task DeleteAsync(StockItem entity, CancellationToken ct) { db.StockItems.Remove(entity); await db.SaveChangesAsync(ct); }
}
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration) { var connection = configuration.GetConnectionString("InventoryDatabase") ?? throw new InvalidOperationException("ConnectionStrings:InventoryDatabase is required."); services.AddDbContext<InventoryDbContext>(o => o.UseNpgsql(connection)); services.AddScoped<IStockItemRepository, StockItemRepository>(); return services; }
}
public sealed class DesignTimeFactory : IDesignTimeDbContextFactory<InventoryDbContext>
{
    public InventoryDbContext CreateDbContext(string[] args) { var builder = new DbContextOptionsBuilder<InventoryDbContext>(); builder.UseNpgsql("Host=localhost;Database=orderflow_inventory;Username=postgres;Password=postgres"); return new InventoryDbContext(builder.Options); }
}
