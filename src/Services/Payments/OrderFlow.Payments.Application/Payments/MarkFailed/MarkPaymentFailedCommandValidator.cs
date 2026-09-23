namespace OrderFlow.Payments.Application.Payments;

public sealed class MarkPaymentFailedCommandValidator
    : AbstractValidator<PaymentFeatures.MarkPaymentFailedCommand>
{
    public MarkPaymentFailedCommandValidator() =>
        RuleFor(candidate => candidate.Reason).NotEmpty().MaximumLength(1000);
}
