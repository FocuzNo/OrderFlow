using Microsoft.EntityFrameworkCore;
using OrderFlow.Payments.Application.Abstractions.Persistence;
using OrderFlow.Payments.Domain.Payments;

namespace OrderFlow.Payments.Infrastructure.Persistence;

public sealed class PaymentsDbContext(DbContextOptions<PaymentsDbContext> options)
    : DbContext(options),
        IUnitOfWork
{
    public DbSet<Payment> Payments => Set<Payment>();

    public DbSet<Refund> Refunds => Set<Refund>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PaymentsDbContext).Assembly);
}
