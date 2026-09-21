using OrderFlow.Payments.Domain.Payments;
namespace OrderFlow.Payments.Application.Abstractions.Persistence;
public interface IPaymentRepository { Task<Payment?> GetAsync(Guid id,CancellationToken ct); Task<Payment?> GetByOrderAsync(Guid orderId,CancellationToken ct); Task AddAsync(Payment payment,CancellationToken ct); Task SaveAsync(CancellationToken ct); }
