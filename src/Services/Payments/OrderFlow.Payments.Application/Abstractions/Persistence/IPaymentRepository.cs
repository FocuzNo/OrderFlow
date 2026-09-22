using OrderFlow.Payments.Domain.Payments;

namespace OrderFlow.Payments.Application.Abstractions.Persistence;

public interface IPaymentRepository : IRepository<Payment>
{
    Task<IReadOnlyList<Payment>> ListAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken
    );
    Task<Payment?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<Payment?> GetByOrderAsync(Guid orderId, CancellationToken cancellationToken);
}
