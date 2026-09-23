using F = OrderFlow.Payments.Application.Payments.PaymentFeatures;

namespace OrderFlow.Payments.Api.Endpoints;

public static partial class PaymentEndpoints
{
    public sealed class ProcessPaymentEndpoint(ISender sender)
        : Endpoint<PaymentIdRequest, F.PaymentResponse>
    {
        public override void Configure()
        {
            Post("/api/payments/{id}/process");
            AllowAnonymous();
        }

        public override async Task HandleAsync(
            PaymentIdRequest request,
            CancellationToken cancellationToken
        ) =>
            await Send.OkAsync(
                await sender.Send(new F.ProcessPaymentCommand(request.Id), cancellationToken),
                cancellationToken
            );
    }
}
