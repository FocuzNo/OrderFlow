namespace OrderFlow.Inventory.Application.Inventory;

public static partial class InventoryFeatures
{
    public sealed class GetReservationByIdQueryValidator
        : AbstractValidator<GetReservationByIdQuery>
    {
        public GetReservationByIdQueryValidator() => RuleFor(query => query.Id).NotEmpty();
    }
}
