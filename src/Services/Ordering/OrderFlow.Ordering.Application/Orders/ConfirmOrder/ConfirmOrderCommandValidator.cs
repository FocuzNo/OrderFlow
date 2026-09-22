namespace OrderFlow.Ordering.Application.Orders;

public static partial class OrderFeatures
{
    public sealed class ConfirmOrderCommandValidator : AbstractValidator<ConfirmOrderCommand>
    {
        public ConfirmOrderCommandValidator()
        {
            RuleFor(command => command.OrderId).NotEmpty();
        }
    }
}
