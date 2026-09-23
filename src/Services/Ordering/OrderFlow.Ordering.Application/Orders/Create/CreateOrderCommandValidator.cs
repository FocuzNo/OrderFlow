namespace OrderFlow.Ordering.Application.Orders;

public sealed class CreateOrderCommandValidator
    : AbstractValidator<OrderFeatures.CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(command => command.Items).NotEmpty();
        RuleForEach(command => command.Items)
            .NotNull()
            .ChildRules(item =>
            {
                item.RuleFor(value => value.ProductId).NotEmpty();
                item.RuleFor(value => value.ProductName).NotEmpty().MaximumLength(200);
                item.RuleFor(value => value.UnitPrice).GreaterThanOrEqualTo(0);
                item.RuleFor(value => value.Quantity).GreaterThan(0);
            });
        RuleFor(command => command.ShippingAddress).NotNull();
        RuleFor(candidate => candidate.CustomerId).NotEmpty();
        RuleFor(candidate => candidate.CustomerEmail).NotEmpty().EmailAddress().MaximumLength(320);
        RuleFor(candidate => candidate.ShippingAddress.Line1)
            .NotEmpty()
            .When(command => command.ShippingAddress is not null);
        RuleFor(candidate => candidate.ShippingAddress.City)
            .NotEmpty()
            .When(command => command.ShippingAddress is not null);
        RuleFor(candidate => candidate.ShippingAddress.PostalCode)
            .NotEmpty()
            .When(command => command.ShippingAddress is not null);
        RuleFor(candidate => candidate.ShippingAddress.Country)
            .NotEmpty()
            .When(command => command.ShippingAddress is not null);
    }
}
