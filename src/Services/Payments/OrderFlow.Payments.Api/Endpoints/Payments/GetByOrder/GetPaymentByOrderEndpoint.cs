using FastEndpoints;
using MediatR;
using F = OrderFlow.Payments.Application.Payments.PaymentFeatures;

namespace OrderFlow.Payments.Api.Endpoints;

public static partial class PaymentEndpoints
{
    public sealed class GetPaymentByOrderEndpoint(ISender s)
        : Endpoint<OrderIdRequest, F.PaymentResponse>
    {
        public override void Configure()
        {
            Get("/api/orders/{id}/payment");
            AllowAnonymous();
        }

        public override async Task HandleAsync(OrderIdRequest r, CancellationToken ct) =>
            await Send.OkAsync(await s.Send(new F.GetPaymentByOrderQuery(r.Id), ct), ct);
    }
}
