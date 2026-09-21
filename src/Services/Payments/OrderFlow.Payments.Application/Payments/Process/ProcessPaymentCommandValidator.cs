namespace OrderFlow.Payments.Application.Payments;

public static partial class PaymentFeatures
{
    public sealed class ProcessPaymentCommandValidator : AbstractValidator<ProcessPaymentCommand>
    {
        public ProcessPaymentCommandValidator() =>
            RuleFor(candidate => candidate.PaymentId).NotEmpty();
    }
}
