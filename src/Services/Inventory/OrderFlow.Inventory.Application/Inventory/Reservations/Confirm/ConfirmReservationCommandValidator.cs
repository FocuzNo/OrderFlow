namespace OrderFlow.Inventory.Application.Inventory;

public static partial class InventoryFeatures
{
    public sealed class ConfirmReservationCommandValidator
        : AbstractValidator<ConfirmReservationCommand>
    {
        public ConfirmReservationCommandValidator()
        {
            RuleFor(x => x.ProductId).NotEmpty();
            RuleFor(x => x.WarehouseId).NotEmpty();
            RuleFor(x => x.ReservationId).NotEmpty();
        }
    }
}
