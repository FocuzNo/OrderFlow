using FastEndpoints;
using MediatR;
using F = OrderFlow.Payments.Application.Payments.PaymentFeatures;

namespace OrderFlow.Payments.Api.Endpoints;

public static partial class PaymentEndpoints
{
    public sealed class ProcessPaymentEndpoint(ISender s)
        : Endpoint<PaymentIdRequest, F.PaymentResponse>
    {
        public override void Configure()
        {
            Post("/api/payments/{id}/process");
            AllowAnonymous();
        }

        public override async Task HandleAsync(PaymentIdRequest r, CancellationToken ct) =>
            await Send.OkAsync(await s.Send(new F.ProcessPaymentCommand(r.Id), ct), ct);
    }
}
