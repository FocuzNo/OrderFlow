using OrderFlow.Payments.Application.Abstractions.Messaging;

namespace OrderFlow.Payments.Application.Payments;

public static partial class PaymentFeatures
{
    public sealed record GetPaymentsQuery(int Page = 1, int PageSize = 20)
        : IQuery<IReadOnlyList<PaymentResponse>>;
}
