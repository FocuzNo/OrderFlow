using OrderFlow.Payments.Application.Abstractions.Errors;
using OrderFlow.Payments.Application.Abstractions.Messaging;
using OrderFlow.Payments.Application.Abstractions.Payments;
using OrderFlow.Payments.Application.Abstractions.Persistence;
using OrderFlow.Payments.Domain.Payments;

namespace OrderFlow.Payments.Application.Payments;

public static partial class PaymentFeatures
{
    public sealed class GetPaymentByOrderQueryHandler(IPaymentRepository repository)
        : IRequestHandler<GetPaymentByOrderQuery, PaymentResponse>
    {
        public async Task<PaymentResponse> Handle(
            GetPaymentByOrderQuery q,
            CancellationToken cancellationToken
        ) =>
            Map(
                await repository.GetByOrderAsync(q.OrderId, cancellationToken)
                    ?? throw new NotFoundException("Payment was not found.")
            );
    }
}
