using OrderFlow.Payments.Application.Abstractions.Persistence;
using OrderFlow.Payments.Domain.Payments;
using OrderFlow.Payments.Infrastructure.Persistence.Repositories;

namespace OrderFlow.Payments.Infrastructure.Persistence;

public sealed class PaymentRepository(PaymentsDbContext databaseContext)
    : Repository<Payment>(databaseContext),
        IPaymentRepository
{
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
