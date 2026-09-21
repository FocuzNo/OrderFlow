using FastEndpoints;
using MediatR;
using F = OrderFlow.Payments.Application.Payments.PaymentFeatures;

namespace OrderFlow.Payments.Api.Endpoints;

public static partial class PaymentEndpoints
{
    public sealed class MarkPaymentFailedRequest
    {
        public Guid Id { get; set; }

        public string Reason { get; set; } = string.Empty;
    }
}
