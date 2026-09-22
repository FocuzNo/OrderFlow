namespace OrderFlow.Ordering.Application.Orders;

public static partial class OrderFeatures
{
    public sealed class GetOrdersQueryValidator : AbstractValidator<GetOrdersQuery>
    {
        public GetOrdersQueryValidator()
        {
            RuleFor(query => query.Page).GreaterThan(0);
            RuleFor(query => query.PageSize).InclusiveBetween(1, 100);
        }
    }
}
