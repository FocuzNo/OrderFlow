namespace OrderFlow.Ordering.Application.Orders;

public sealed class CreateOrderCommandValidator
    : AbstractValidator<OrderFeatures.CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(candidate => candidate.CustomerId).NotEmpty();
        RuleFor(candidate => candidate.CustomerEmail).NotEmpty().EmailAddress().MaximumLength(320);
        RuleFor(candidate => candidate.ShippingAddress.Line1).NotEmpty();
        RuleFor(candidate => candidate.ShippingAddress.City).NotEmpty();
        RuleFor(candidate => candidate.ShippingAddress.PostalCode).NotEmpty();
        RuleFor(candidate => candidate.ShippingAddress.Country).NotEmpty();
    }
}
