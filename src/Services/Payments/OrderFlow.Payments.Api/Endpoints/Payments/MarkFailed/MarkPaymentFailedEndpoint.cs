using F = OrderFlow.Payments.Application.Payments.PaymentFeatures;

namespace OrderFlow.Payments.Api.Endpoints;

public static partial class PaymentEndpoints
{
    public sealed class MarkPaymentFailedEndpoint(ISender sender)
        : Endpoint<MarkPaymentFailedRequest>
    {
        public override void Configure()
        {
            Post("/api/payments/{id}/fail");
            AllowAnonymous();
        }

        public override async Task HandleAsync(
            MarkPaymentFailedRequest request,
            CancellationToken cancellationToken
        )
        {
            await sender.Send(
                new F.MarkPaymentFailedCommand(request.Id, request.Reason),
                cancellationToken
            );
            await Send.NoContentAsync(cancellationToken);
        }
    }
}
