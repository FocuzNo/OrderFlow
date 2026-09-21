using F = OrderFlow.Payments.Application.Payments.PaymentFeatures;

namespace OrderFlow.Payments.Api.Endpoints;

public static partial class PaymentEndpoints
{
    public sealed record MarkPaymentSucceededRequest
    {
        public Guid Id { get; init; }

        public string Reference { get; init; } = string.Empty;
    }
}
