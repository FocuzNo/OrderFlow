namespace OrderFlow.Payments.Application.Payments;

public static partial class PaymentFeatures
{
    public sealed class GetPaymentByIdQueryValidator : AbstractValidator<GetPaymentByIdQuery>
    {
        public GetPaymentByIdQueryValidator() => RuleFor(query => query.Id).NotEmpty();
    }
}
