using FastEndpoints;
using MediatR;
using F = OrderFlow.Payments.Application.Payments.PaymentFeatures;

namespace OrderFlow.Payments.Api.Endpoints;

public static partial class PaymentEndpoints
{
    public sealed class MarkPaymentSucceededRequest
    {
        public Guid Id { get; set; }

        public string Reference { get; set; } = string.Empty;
    }
}
