using F = OrderFlow.Payments.Application.Payments.PaymentFeatures;

namespace OrderFlow.Payments.Api.Endpoints;

public static partial class PaymentEndpoints
{
    public sealed class RefundPaymentEndpoint(ISender sender)
        : Endpoint<RefundPaymentRequest, F.PaymentResponse>
    {
        public override void Configure()
        {
            Post("/api/payments/{id}/refunds");
            AllowAnonymous();
        }

        public override async Task HandleAsync(
            RefundPaymentRequest request,
            CancellationToken cancellationToken
        ) =>
            await Send.OkAsync(
                await sender.Send(
                    new F.RefundPaymentCommand(request.Id, request.Amount, request.Reason),
                    cancellationToken
                ),
                cancellationToken
            );
    }
}
