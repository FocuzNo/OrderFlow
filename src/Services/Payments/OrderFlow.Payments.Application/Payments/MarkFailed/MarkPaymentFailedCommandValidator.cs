using FluentValidation;

namespace OrderFlow.Payments.Application.Payments;

public sealed class MarkPaymentFailedCommandValidator
    : AbstractValidator<PaymentFeatures.MarkPaymentFailedCommand>
{
    public MarkPaymentFailedCommandValidator() =>
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(1000);
}
