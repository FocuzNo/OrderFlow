using FastEndpoints;
using MediatR;
using F = OrderFlow.Payments.Application.Payments.PaymentFeatures;

namespace OrderFlow.Payments.Api.Endpoints;

public static partial class PaymentEndpoints
{
    public sealed class CreatePaymentRequest
    {
        public Guid OrderId { get; set; }

        public decimal Amount { get; set; }

        public string Method { get; set; } = "Card";
    }
}
