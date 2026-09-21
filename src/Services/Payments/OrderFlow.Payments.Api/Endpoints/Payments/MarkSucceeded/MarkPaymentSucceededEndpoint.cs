using FastEndpoints;
using MediatR;
using F = OrderFlow.Payments.Application.Payments.PaymentFeatures;

namespace OrderFlow.Payments.Api.Endpoints;

public static partial class PaymentEndpoints
{
    public sealed class MarkPaymentSucceededEndpoint(ISender s)
        : Endpoint<MarkPaymentSucceededRequest>
    {
        public override void Configure()
        {
            Post("/api/payments/{id}/succeed");
            AllowAnonymous();
        }

        public override async Task HandleAsync(MarkPaymentSucceededRequest r, CancellationToken ct)
        {
            await s.Send(new F.MarkPaymentSucceededCommand(r.Id, r.Reference), ct);
            await Send.NoContentAsync(ct);
        }
    }
}
