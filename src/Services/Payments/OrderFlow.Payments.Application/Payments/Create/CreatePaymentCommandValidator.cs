using FluentValidation;

namespace OrderFlow.Payments.Application.Payments;

public sealed class CreatePaymentCommandValidator
    : AbstractValidator<PaymentFeatures.CreatePaymentCommand>
{
    public CreatePaymentCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.Method).NotEmpty();
    }
}
