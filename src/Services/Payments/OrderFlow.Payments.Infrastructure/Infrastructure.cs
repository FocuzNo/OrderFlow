using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderFlow.Payments.Application;
using OrderFlow.Payments.Domain;

namespace OrderFlow.Payments.Infrastructure;

public sealed class OutboxMessage { public Guid Id { get; set; } public string Type { get; set; } = string.Empty; public string Payload { get; set; } = string.Empty; public DateTimeOffset OccurredAt { get; set; } public DateTimeOffset? ProcessedAt { get; set; } public string? Error { get; set; } }
public sealed class PaymentsDbContext(DbContextOptions<PaymentsDbContext> options) : DbContext(options)
{
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        var aggregates = ChangeTracker.Entries<IHasDomainEvents>().Select(x => x.Entity).Where(x => x.DomainEvents.Count > 0).ToArray();
        foreach (var aggregate in aggregates) { foreach (var item in aggregate.DomainEvents) OutboxMessages.Add(new OutboxMessage { Id = item.Id, Type = item.Type, Payload = JsonSerializer.Serialize(item), OccurredAt = item.OccurredAt }); aggregate.ClearDomainEvents(); }
        return await base.SaveChangesAsync(ct);
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Payment>(b => { b.ToTable("payments"); b.HasKey(x => x.Id); b.Property(x => x.Reference).HasMaxLength(Payment.MaxReferenceLength).IsRequired(); b.Property(x => x.Description).HasMaxLength(Payment.MaxDescriptionLength); b.Property(x => x.Value).HasPrecision(18, 2); b.Property(x => x.Status).HasMaxLength(50).IsRequired(); b.Ignore(x => x.DomainEvents); });
        modelBuilder.Entity<OutboxMessage>(b => { b.ToTable("outbox_messages"); b.HasKey(x => x.Id); b.Property(x => x.Type).HasMaxLength(250).IsRequired(); b.Property(x => x.Payload).HasColumnType("jsonb").IsRequired(); b.Property(x => x.Error).HasMaxLength(2000); b.HasIndex(x => new { x.ProcessedAt, x.OccurredAt }); });
    }
}
public sealed class PaymentRepository(PaymentsDbContext db) : IPaymentRepository
{
    public Task<Payment?> GetAsync(Guid id, CancellationToken ct) => db.Payments.SingleOrDefaultAsync(x => x.Id == id, ct);
    public async Task<IReadOnlyList<Payment>> ListAsync(CancellationToken ct) => await db.Payments.AsNoTracking().OrderBy(x => x.CreatedAt).ToListAsync(ct);
    public async Task AddAsync(Payment entity, CancellationToken ct) { db.Payments.Add(entity); await db.SaveChangesAsync(ct); }
    public async Task SaveAsync(CancellationToken ct) => await db.SaveChangesAsync(ct);
    public async Task DeleteAsync(Payment entity, CancellationToken ct) { db.Payments.Remove(entity); await db.SaveChangesAsync(ct); }
}
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration) { var connection = configuration.GetConnectionString("PaymentsDatabase") ?? throw new InvalidOperationException("ConnectionStrings:PaymentsDatabase is required."); services.AddDbContext<PaymentsDbContext>(o => o.UseNpgsql(connection)); services.AddScoped<IPaymentRepository, PaymentRepository>(); return services; }
}
public sealed class DesignTimeFactory : IDesignTimeDbContextFactory<PaymentsDbContext>
{
    public PaymentsDbContext CreateDbContext(string[] args) { var builder = new DbContextOptionsBuilder<PaymentsDbContext>(); builder.UseNpgsql("Host=localhost;Database=orderflow_payments;Username=postgres;Password=postgres"); return new PaymentsDbContext(builder.Options); }
}
