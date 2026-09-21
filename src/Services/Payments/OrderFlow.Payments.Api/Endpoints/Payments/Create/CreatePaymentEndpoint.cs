using FastEndpoints;
using MediatR;
using F = OrderFlow.Payments.Application.Payments.PaymentFeatures;

namespace OrderFlow.Payments.Api.Endpoints;

public static partial class PaymentEndpoints
{
    public sealed class CreatePaymentEndpoint(ISender s)
        : Endpoint<CreatePaymentRequest, F.PaymentResponse>
    {
        public override void Configure()
        {
            Post("/api/payments");
            AllowAnonymous();
        }

        public override async Task HandleAsync(CreatePaymentRequest r, CancellationToken ct) =>
            await Send.ResponseAsync(
                await s.Send(new F.CreatePaymentCommand(r.OrderId, r.Amount, r.Method), ct),
                201,
                ct
            );
    }
}
