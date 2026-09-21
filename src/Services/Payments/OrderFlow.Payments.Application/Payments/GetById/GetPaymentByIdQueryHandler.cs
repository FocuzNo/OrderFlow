using MediatR;
using OrderFlow.Payments.Application.Abstractions.Errors;
using OrderFlow.Payments.Application.Abstractions.Messaging;
using OrderFlow.Payments.Application.Abstractions.Payments;
using OrderFlow.Payments.Application.Abstractions.Persistence;
using OrderFlow.Payments.Domain.Payments;

namespace OrderFlow.Payments.Application.Payments;

public static partial class PaymentFeatures
{
    public sealed class GetPaymentByIdQueryHandler(IPaymentRepository r)
        : IRequestHandler<GetPaymentByIdQuery, PaymentResponse>
    {
        public async Task<PaymentResponse> Handle(GetPaymentByIdQuery q, CancellationToken ct) =>
            Map(await Find(r, q.Id, ct));
    }
}
