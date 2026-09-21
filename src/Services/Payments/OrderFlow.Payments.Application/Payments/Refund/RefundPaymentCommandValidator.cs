using FluentValidation;

namespace OrderFlow.Payments.Application.Payments;

public sealed class RefundPaymentCommandValidator
    : AbstractValidator<PaymentFeatures.RefundPaymentCommand>
{
    public RefundPaymentCommandValidator()
    {
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(1000);
    }
}
