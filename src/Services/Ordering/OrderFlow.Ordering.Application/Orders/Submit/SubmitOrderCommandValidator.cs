namespace OrderFlow.Ordering.Application.Orders;

public static partial class OrderFeatures
{
    public sealed class SubmitOrderCommandValidator : AbstractValidator<SubmitOrderCommand>
    {
        public SubmitOrderCommandValidator() => RuleFor(candidate => candidate.OrderId).NotEmpty();
    }
}
