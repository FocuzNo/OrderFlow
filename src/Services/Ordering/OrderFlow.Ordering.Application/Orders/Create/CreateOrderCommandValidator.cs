using FluentValidation;

namespace OrderFlow.Ordering.Application.Orders;

public sealed class CreateOrderCommandValidator
    : AbstractValidator<OrderFeatures.CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.CustomerEmail).NotEmpty().EmailAddress().MaximumLength(320);
        RuleFor(x => x.ShippingAddress.Line1).NotEmpty();
        RuleFor(x => x.ShippingAddress.City).NotEmpty();
        RuleFor(x => x.ShippingAddress.PostalCode).NotEmpty();
        RuleFor(x => x.ShippingAddress.Country).NotEmpty();
    }
}
