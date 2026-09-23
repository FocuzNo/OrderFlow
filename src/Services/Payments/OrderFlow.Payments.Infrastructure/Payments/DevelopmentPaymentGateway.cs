using Microsoft.Extensions.Options;
using OrderFlow.Payments.Application.Abstractions.Payments;

namespace OrderFlow.Payments.Infrastructure.Payments;

public sealed class DevelopmentPaymentGateway(IOptions<DevelopmentPaymentGatewayOptions> options)
    : IPaymentGateway
{
    public Task<PaymentGatewayResult> ChargeAsync(
        Guid id,
        decimal amount,
        CancellationToken cancellationToken
    )
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(
            options.Value.Succeed
                ? new PaymentGatewayResult(true, $"DEV-{id:N}", null)
                : new PaymentGatewayResult(false, null, "Configured development decline.")
        );
    }
}
