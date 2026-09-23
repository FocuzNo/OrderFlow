namespace OrderFlow.Ordering.Application.Orders;

public static partial class OrderFeatures
{
    public sealed class RemoveOrderItemCommandValidator : AbstractValidator<RemoveOrderItemCommand>
    {
        public RemoveOrderItemCommandValidator()
        {
            RuleFor(candidate => candidate.OrderId).NotEmpty();
            RuleFor(candidate => candidate.ItemId).NotEmpty();
        }
    }
}
