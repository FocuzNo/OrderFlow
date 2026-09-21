namespace OrderFlow.Payments.Application.Payments;

public static partial class PaymentFeatures
{
    public sealed class GetPaymentByOrderQueryValidator : AbstractValidator<GetPaymentByOrderQuery>
    {
        public GetPaymentByOrderQueryValidator() => RuleFor(query => query.OrderId).NotEmpty();
    }
}
