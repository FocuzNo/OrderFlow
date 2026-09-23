namespace OrderFlow.Payments.Application.Payments;

public static partial class PaymentFeatures
{
    public sealed class GetPaymentsQueryValidator : AbstractValidator<GetPaymentsQuery>
    {
        public GetPaymentsQueryValidator()
        {
            RuleFor(query => query.Page).GreaterThan(0);
            RuleFor(query => query.PageSize).InclusiveBetween(1, 100);
        }
    }
}
