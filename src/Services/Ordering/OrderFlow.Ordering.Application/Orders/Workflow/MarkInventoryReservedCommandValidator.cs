namespace OrderFlow.Ordering.Application.Orders;

public static partial class OrderFeatures
{
    public sealed class MarkInventoryReservedCommandValidator
        : AbstractValidator<MarkInventoryReservedCommand>
    {
        public MarkInventoryReservedCommandValidator() =>
            RuleFor(command => command.OrderId).NotEmpty();
    }
}
