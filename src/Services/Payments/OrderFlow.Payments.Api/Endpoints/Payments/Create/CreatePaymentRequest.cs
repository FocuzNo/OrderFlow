using F = OrderFlow.Payments.Application.Payments.PaymentFeatures;

namespace OrderFlow.Payments.Api.Endpoints;

public static partial class PaymentEndpoints
{
    public sealed record CreatePaymentRequest
    {
        public bool SimulateFailure { get; init; }

        public Guid OrderId { get; init; }

        public decimal Amount { get; init; }

        public string Method { get; init; } = "Card";
    }
}
