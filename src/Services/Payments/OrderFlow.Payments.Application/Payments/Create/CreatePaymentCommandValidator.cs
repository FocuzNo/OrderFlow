namespace OrderFlow.Payments.Application.Payments;

public sealed class CreatePaymentCommandValidator
    : AbstractValidator<PaymentFeatures.CreatePaymentCommand>
{
    public CreatePaymentCommandValidator()
    {
        RuleFor(candidate => candidate.OrderId).NotEmpty();
        RuleFor(candidate => candidate.Amount).GreaterThan(0).PrecisionScale(18, 2, true);
        RuleFor(candidate => candidate.Method)
            .NotEmpty()
            .Must(method =>
                OrderFlow.Payments.Domain.Payments.PaymentMethod.List.Any(value =>
                    string.Equals(value.Name, method, StringComparison.OrdinalIgnoreCase)
                )
            );
    }
}
