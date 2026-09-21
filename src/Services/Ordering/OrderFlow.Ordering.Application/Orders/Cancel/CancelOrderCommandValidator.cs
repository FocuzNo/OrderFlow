using FluentValidation;

namespace OrderFlow.Ordering.Application.Orders;

public sealed class CancelOrderCommandValidator
    : AbstractValidator<OrderFeatures.CancelOrderCommand>
{
    public CancelOrderCommandValidator() => RuleFor(x => x.Reason).NotEmpty().MaximumLength(1000);
}
