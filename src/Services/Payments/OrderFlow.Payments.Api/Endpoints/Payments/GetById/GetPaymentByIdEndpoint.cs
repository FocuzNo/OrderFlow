using FastEndpoints;
using MediatR;
using F = OrderFlow.Payments.Application.Payments.PaymentFeatures;

namespace OrderFlow.Payments.Api.Endpoints;

public static partial class PaymentEndpoints
{
    public sealed class GetPaymentByIdEndpoint(ISender s)
        : Endpoint<PaymentIdRequest, F.PaymentResponse>
    {
        public override void Configure()
        {
            Get("/api/payments/{id}");
            AllowAnonymous();
        }

        public override async Task HandleAsync(PaymentIdRequest r, CancellationToken ct) =>
            await Send.OkAsync(await s.Send(new F.GetPaymentByIdQuery(r.Id), ct), ct);
    }
}
