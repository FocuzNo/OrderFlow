using FastEndpoints;
using MediatR;
using F = OrderFlow.Payments.Application.Payments.PaymentFeatures;

namespace OrderFlow.Payments.Api.Endpoints;

public static partial class PaymentEndpoints
{
    public sealed class RefundPaymentEndpoint(ISender s)
        : Endpoint<RefundPaymentRequest, F.PaymentResponse>
    {
        public override void Configure()
        {
            Post("/api/payments/{id}/refunds");
            AllowAnonymous();
        }

        public override async Task HandleAsync(RefundPaymentRequest r, CancellationToken ct) =>
            await Send.OkAsync(
                await s.Send(new F.RefundPaymentCommand(r.Id, r.Amount, r.Reason), ct),
                ct
            );
    }
}
