using F = OrderFlow.Payments.Application.Payments.PaymentFeatures;

namespace OrderFlow.Payments.Api.Endpoints;

public static partial class PaymentEndpoints
{
    public sealed class GetPaymentByIdEndpoint(ISender sender)
        : Endpoint<PaymentIdRequest, F.PaymentResponse>
    {
        public override void Configure()
        {
            Get("/api/payments/{id}");
            AllowAnonymous();
        }

        public override async Task HandleAsync(
            PaymentIdRequest request,
            CancellationToken cancellationToken
        ) =>
            await Send.OkAsync(
                await sender.Send(new F.GetPaymentByIdQuery(request.Id), cancellationToken),
                cancellationToken
            );
    }
}
