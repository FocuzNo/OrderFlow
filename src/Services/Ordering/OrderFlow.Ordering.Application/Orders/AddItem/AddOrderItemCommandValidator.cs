using FluentValidation;

namespace OrderFlow.Ordering.Application.Orders;

public sealed class AddOrderItemCommandValidator
    : AbstractValidator<OrderFeatures.AddOrderItemCommand>
{
    public AddOrderItemCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.ProductName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.UnitPrice).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Quantity).GreaterThan(0);
    }
}
