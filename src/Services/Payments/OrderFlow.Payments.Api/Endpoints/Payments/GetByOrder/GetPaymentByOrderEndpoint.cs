using F = OrderFlow.Payments.Application.Payments.PaymentFeatures;

namespace OrderFlow.Payments.Api.Endpoints;

public static partial class PaymentEndpoints
{
    public sealed class GetPaymentByOrderEndpoint(ISender sender)
        : Endpoint<OrderIdRequest, F.PaymentResponse>
    {
        public override void Configure()
        {
            Get("/api/payments/order/{id}");
            AllowAnonymous();
        }

        public override async Task HandleAsync(
            OrderIdRequest request,
            CancellationToken cancellationToken
        ) =>
            await Send.OkAsync(
                await sender.Send(new F.GetPaymentByOrderQuery(request.Id), cancellationToken),
                cancellationToken
            );
    }
}
