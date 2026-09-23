namespace OrderFlow.Payments.Application.Payments;

public sealed class MarkPaymentSucceededCommandValidator
    : AbstractValidator<PaymentFeatures.MarkPaymentSucceededCommand>
{
    public MarkPaymentSucceededCommandValidator() =>
        RuleFor(candidate => candidate.Reference).NotEmpty().MaximumLength(200);
}
