namespace OrderFlow.Payments.Application.Payments;

public sealed class RefundPaymentCommandValidator
    : AbstractValidator<PaymentFeatures.RefundPaymentCommand>
{
    public RefundPaymentCommandValidator()
    {
        RuleFor(candidate => candidate.Amount).GreaterThan(0);
        RuleFor(candidate => candidate.Reason).NotEmpty().MaximumLength(1000);
    }
}
