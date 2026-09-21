using FluentValidation;

namespace OrderFlow.Payments.Application.Payments;

public sealed class CreatePaymentValidator : AbstractValidator<PaymentFeatures.Create>
{
    public CreatePaymentValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.Method).NotEmpty();
    }
}

public sealed class MarkPaymentSucceededValidator : AbstractValidator<PaymentFeatures.MarkSucceeded>
{
    public MarkPaymentSucceededValidator() =>
        RuleFor(x => x.Reference).NotEmpty().MaximumLength(200);
}

public sealed class MarkPaymentFailedValidator : AbstractValidator<PaymentFeatures.MarkFailed>
{
    public MarkPaymentFailedValidator() => RuleFor(x => x.Reason).NotEmpty().MaximumLength(1000);
}

public sealed class RefundPaymentValidator : AbstractValidator<PaymentFeatures.RefundPayment>
{
    public RefundPaymentValidator()
    {
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(1000);
    }
}
