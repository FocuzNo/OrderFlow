namespace OrderFlow.Inventory.Application.Inventory;

public static partial class InventoryFeatures
{
    public sealed class ReleaseReservationCommandValidator
        : AbstractValidator<ReleaseReservationCommand>
    {
        public ReleaseReservationCommandValidator()
        {
            RuleFor(x => x.ProductId).NotEmpty();
            RuleFor(x => x.WarehouseId).NotEmpty();
            RuleFor(x => x.ReservationId).NotEmpty();
        }
    }
}
