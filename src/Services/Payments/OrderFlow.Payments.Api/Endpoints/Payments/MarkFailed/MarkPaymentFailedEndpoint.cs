using FastEndpoints;
using MediatR;
using F = OrderFlow.Payments.Application.Payments.PaymentFeatures;

namespace OrderFlow.Payments.Api.Endpoints;

public static partial class PaymentEndpoints
{
    public sealed class MarkPaymentFailedEndpoint(ISender s) : Endpoint<MarkPaymentFailedRequest>
    {
        public override void Configure()
        {
            Post("/api/payments/{id}/fail");
            AllowAnonymous();
        }

        public override async Task HandleAsync(MarkPaymentFailedRequest r, CancellationToken ct)
        {
            await s.Send(new F.MarkPaymentFailedCommand(r.Id, r.Reason), ct);
            await Send.NoContentAsync(ct);
        }
    }
}
