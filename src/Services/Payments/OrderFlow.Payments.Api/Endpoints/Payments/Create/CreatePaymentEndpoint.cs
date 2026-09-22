using F = OrderFlow.Payments.Application.Payments.PaymentFeatures;

namespace OrderFlow.Payments.Api.Endpoints;

public static partial class PaymentEndpoints
{
    public sealed class CreatePaymentEndpoint(ISender sender)
        : Endpoint<CreatePaymentRequest, F.PaymentResponse>
    {
        public override void Configure()
        {
            Post("/api/payments");
            AllowAnonymous();
        }

        public override async Task HandleAsync(
            CreatePaymentRequest request,
            CancellationToken cancellationToken
        ) =>
            await Send.ResponseAsync(
                await sender.Send(
                    new F.CreatePaymentCommand(
                        request.OrderId,
                        request.Amount,
                        request.Method,
                        request.SimulateFailure
                    ),
                    cancellationToken
                ),
                201,
                cancellationToken
            );
    }
}
