using OrderFlow.Payments.Domain.Payments;

namespace OrderFlow.Payments.Application.Abstractions.Persistence;

public interface IPaymentRepository : IRepository<Payment>
{
    Task<Payment?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<Payment?> GetByOrderAsync(Guid orderId, CancellationToken cancellationToken);
}
