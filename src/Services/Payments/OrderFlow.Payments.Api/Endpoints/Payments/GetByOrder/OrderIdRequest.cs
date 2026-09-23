using F = OrderFlow.Payments.Application.Payments.PaymentFeatures;

namespace OrderFlow.Payments.Api.Endpoints;

public static partial class PaymentEndpoints
{
    public sealed record OrderIdRequest
    {
        public Guid Id { get; init; }
    }
}
