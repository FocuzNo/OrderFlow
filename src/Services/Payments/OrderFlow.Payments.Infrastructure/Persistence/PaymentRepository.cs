using Microsoft.EntityFrameworkCore;
using OrderFlow.Payments.Application.Abstractions.Persistence;
using OrderFlow.Payments.Domain.Payments;

namespace OrderFlow.Payments.Infrastructure.Persistence;

public sealed class PaymentRepository(PaymentsDbContext db) : IPaymentRepository
{
    public Task<Payment?> GetAsync(Guid id, CancellationToken ct) =>
        db.Payments.Include(x => x.Refunds).SingleOrDefaultAsync(x => x.Id == id, ct);

    public Task<Payment?> GetByOrderAsync(Guid id, CancellationToken ct) =>
        db
            .Payments.AsNoTracking()
            .Include(x => x.Refunds)
            .SingleOrDefaultAsync(x => x.OrderId == id, ct);

    public async Task AddAsync(Payment x, CancellationToken ct)
    {
        db.Payments.Add(x);
        await db.SaveChangesAsync(ct);
    }

    public async Task SaveAsync(CancellationToken ct) => await db.SaveChangesAsync(ct);
}
