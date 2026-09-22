using OrderFlow.Payments.Application.Abstractions.Persistence;
using OrderFlow.Payments.Domain.Payments;
using OrderFlow.Payments.Infrastructure.Persistence.Repositories;

namespace OrderFlow.Payments.Infrastructure.Persistence;

public sealed class PaymentRepository(PaymentsDbContext databaseContext)
    : Repository<Payment>(databaseContext),
        IPaymentRepository
{
    public async Task<IReadOnlyList<Payment>> ListAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken
    ) =>
        await DatabaseContext
            .Payments.AsNoTracking()
            .Include(payment => payment.Refunds)
            .OrderBy(entity => entity.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

    public Task<Payment?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        DatabaseContext
            .Payments.Include(candidate => candidate.Refunds)
            .SingleOrDefaultAsync(candidate => candidate.Id == id, cancellationToken);

    public Task<Payment?> GetByOrderAsync(Guid id, CancellationToken cancellationToken) =>
        DatabaseContext
            .Payments.AsNoTracking()
            .Include(candidate => candidate.Refunds)
            .SingleOrDefaultAsync(candidate => candidate.OrderId == id, cancellationToken);
}
