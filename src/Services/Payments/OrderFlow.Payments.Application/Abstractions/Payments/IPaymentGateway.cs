namespace OrderFlow.Payments.Application.Abstractions.Payments;
public interface IPaymentGateway { Task<PaymentGatewayResult> ChargeAsync(Guid paymentId,decimal amount,CancellationToken ct); }
