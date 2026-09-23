using F = OrderFlow.Payments.Application.Payments.PaymentFeatures;

namespace OrderFlow.Payments.Api.Endpoints;

public sealed class GetPaymentsEndpoint(ISender sender)
    : Endpoint<GetPaymentsRequest, IReadOnlyList<F.PaymentResponse>>
{
    public override void Configure()
    {
        Get("/api/payments");
        AllowAnonymous();
    }

    public override async Task HandleAsync(
        GetPaymentsRequest request,
        CancellationToken cancellationToken
    ) =>
        await Send.OkAsync(
            await sender.Send(
                new F.GetPaymentsQuery(request.Page, request.PageSize),
                cancellationToken
            ),
            cancellationToken
        );
}
