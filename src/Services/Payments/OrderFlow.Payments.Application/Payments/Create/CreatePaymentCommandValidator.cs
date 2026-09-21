namespace OrderFlow.Payments.Application.Payments;

public sealed class CreatePaymentCommandValidator
    : AbstractValidator<PaymentFeatures.CreatePaymentCommand>
{
    public CreatePaymentCommandValidator()
    {
        RuleFor(candidate => candidate.OrderId).NotEmpty();
        RuleFor(candidate => candidate.Amount).GreaterThan(0);
        RuleFor(candidate => candidate.Method).NotEmpty();
    }
}
