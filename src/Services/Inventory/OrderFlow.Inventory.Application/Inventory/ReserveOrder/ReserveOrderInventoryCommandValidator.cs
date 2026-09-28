namespace OrderFlow.Inventory.Application.Inventory;

public sealed class ReserveOrderInventoryCommandValidator
    : AbstractValidator<ReserveOrderInventoryCommand>
{
    public ReserveOrderInventoryCommandValidator()
    {
        RuleFor(command => command.EventId).NotEmpty();
        RuleFor(command => command.OrderId).NotEmpty();
        RuleFor(command => command.Items).NotEmpty();
        RuleForEach(command => command.Items).NotNull();
        RuleForEach(command => command.Items).ChildRules(item =>
        {
            item.RuleFor(value => value.ProductId).NotEmpty();
            item.RuleFor(value => value.Quantity).GreaterThan(0);
        });
    }
}
