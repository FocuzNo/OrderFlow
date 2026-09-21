using F = OrderFlow.Payments.Application.Payments.PaymentFeatures;

namespace OrderFlow.Payments.Api.Endpoints;

public static partial class PaymentEndpoints
{
    public sealed class MarkPaymentSucceededEndpoint(ISender sender)
        : Endpoint<MarkPaymentSucceededRequest>
    {
        public override void Configure()
        {
            Post("/api/payments/{id}/succeed");
            AllowAnonymous();
        }

        public override async Task HandleAsync(
            MarkPaymentSucceededRequest request,
            CancellationToken cancellationToken
        )
        {
            await sender.Send(
                new F.MarkPaymentSucceededCommand(request.Id, request.Reference),
                cancellationToken
            );
            await Send.NoContentAsync(cancellationToken);
        }
    }
}
