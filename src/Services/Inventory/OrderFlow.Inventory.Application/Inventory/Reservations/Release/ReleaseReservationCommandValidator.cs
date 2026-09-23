namespace OrderFlow.Inventory.Application.Inventory;

public static partial class InventoryFeatures
{
    public sealed class ReleaseReservationCommandValidator
        : AbstractValidator<ReleaseReservationCommand>
    {
        public ReleaseReservationCommandValidator()
        {
            RuleFor(candidate => candidate.ProductId).NotEmpty();
            RuleFor(candidate => candidate.WarehouseId).NotEmpty();
            RuleFor(candidate => candidate.ReservationId).NotEmpty();
        }
    }
}
