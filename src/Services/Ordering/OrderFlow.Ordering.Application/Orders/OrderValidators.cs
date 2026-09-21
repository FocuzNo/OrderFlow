using FluentValidation;

namespace OrderFlow.Ordering.Application.Orders;

public sealed class CreateOrderValidator : AbstractValidator<OrderFeatures.Create>
{
    public CreateOrderValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.CustomerEmail).NotEmpty().EmailAddress().MaximumLength(320);
        RuleFor(x => x.ShippingAddress.Line1).NotEmpty();
        RuleFor(x => x.ShippingAddress.City).NotEmpty();
        RuleFor(x => x.ShippingAddress.PostalCode).NotEmpty();
        RuleFor(x => x.ShippingAddress.Country).NotEmpty();
    }
}

public sealed class AddOrderItemValidator : AbstractValidator<OrderFeatures.AddItem>
{
    public AddOrderItemValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.ProductName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.UnitPrice).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Quantity).GreaterThan(0);
    }
}

public sealed class CancelOrderValidator : AbstractValidator<OrderFeatures.Cancel>
{
    public CancelOrderValidator() => RuleFor(x => x.Reason).NotEmpty().MaximumLength(1000);
}
