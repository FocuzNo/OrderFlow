namespace OrderFlow.Ordering.Application.Orders;

public static partial class OrderFeatures
{
    public sealed class MarkPaymentProcessingCommandValidator
        : AbstractValidator<MarkPaymentProcessingCommand>
    {
        public MarkPaymentProcessingCommandValidator()
        {
            RuleFor(command => command.OrderId).NotEmpty();
        }
    }
}
