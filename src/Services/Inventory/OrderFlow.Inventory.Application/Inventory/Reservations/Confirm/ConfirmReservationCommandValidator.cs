namespace OrderFlow.Inventory.Application.Inventory;

public static partial class InventoryFeatures
{
    public sealed class ConfirmReservationCommandValidator
        : AbstractValidator<ConfirmReservationCommand>
    {
        public ConfirmReservationCommandValidator()
        {
            RuleFor(candidate => candidate.ProductId).NotEmpty();
            RuleFor(candidate => candidate.WarehouseId).NotEmpty();
            RuleFor(candidate => candidate.ReservationId).NotEmpty();
        }
    }
}
