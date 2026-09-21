namespace OrderFlow.Ordering.Application.Orders;

public sealed class CancelOrderCommandValidator
    : AbstractValidator<OrderFeatures.CancelOrderCommand>
{
    public CancelOrderCommandValidator() =>
        RuleFor(candidate => candidate.Reason).NotEmpty().MaximumLength(1000);
}
