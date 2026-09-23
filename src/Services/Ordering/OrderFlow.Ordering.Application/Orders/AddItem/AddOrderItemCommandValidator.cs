namespace OrderFlow.Ordering.Application.Orders;

public sealed class AddOrderItemCommandValidator
    : AbstractValidator<OrderFeatures.AddOrderItemCommand>
{
    public AddOrderItemCommandValidator()
    {
        RuleFor(candidate => candidate.ProductId).NotEmpty();
        RuleFor(candidate => candidate.ProductName).NotEmpty().MaximumLength(200);
        RuleFor(candidate => candidate.UnitPrice).GreaterThanOrEqualTo(0);
        RuleFor(candidate => candidate.Quantity).GreaterThan(0);
    }
}
