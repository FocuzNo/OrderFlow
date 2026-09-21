using FluentValidation;

namespace OrderFlow.Payments.Application.Payments;

public sealed class MarkPaymentSucceededCommandValidator
    : AbstractValidator<PaymentFeatures.MarkPaymentSucceededCommand>
{
    public MarkPaymentSucceededCommandValidator() =>
        RuleFor(x => x.Reference).NotEmpty().MaximumLength(200);
}
