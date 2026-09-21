using F = OrderFlow.Payments.Application.Payments.PaymentFeatures;

namespace OrderFlow.Payments.Api.Endpoints;

public static partial class PaymentEndpoints
{
    public sealed record MarkPaymentFailedRequest
    {
        public Guid Id { get; init; }

        public string Reason { get; init; } = string.Empty;
    }
}
