using OrderFlow.Payments.Application.Abstractions.Persistence;

namespace OrderFlow.Payments.Application.Payments;

public static partial class PaymentFeatures
{
    public sealed class GetPaymentsQueryHandler(IPaymentRepository repository)
        : IRequestHandler<GetPaymentsQuery, IReadOnlyList<PaymentResponse>>
    {
        public async Task<IReadOnlyList<PaymentResponse>> Handle(
            GetPaymentsQuery query,
            CancellationToken cancellationToken
        ) =>
            (await repository.ListAsync(query.Page, query.PageSize, cancellationToken))
                .Select(Map)
                .ToArray();
    }
}
