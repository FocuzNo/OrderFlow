using OrderFlow.Payments.Application.Abstractions.Errors;
using OrderFlow.Payments.Application.Abstractions.Messaging;
using OrderFlow.Payments.Application.Abstractions.Payments;
using OrderFlow.Payments.Application.Abstractions.Persistence;
using OrderFlow.Payments.Domain.Payments;

namespace OrderFlow.Payments.Application.Payments;

public static partial class PaymentFeatures
{
    public sealed record PaymentResponse(
        Guid Id,
        Guid OrderId,
        decimal Amount,
        string Method,
        string Status,
        string? ProviderReference,
        string? FailureReason,
        IReadOnlyList<RefundResponse> Refunds
    );
}
