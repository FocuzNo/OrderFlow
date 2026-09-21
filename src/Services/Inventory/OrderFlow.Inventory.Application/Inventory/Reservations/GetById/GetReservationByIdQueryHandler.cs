using OrderFlow.Inventory.Application.Abstractions.Errors;
using OrderFlow.Inventory.Application.Abstractions.Messaging;
using OrderFlow.Inventory.Application.Abstractions.Persistence;
using OrderFlow.Inventory.Domain.Stock;
using OrderFlow.Inventory.Domain.Warehouses;

namespace OrderFlow.Inventory.Application.Inventory;

public static partial class InventoryFeatures
{
    public sealed class GetReservationByIdQueryHandler(IInventoryRepository repository)
        : IRequestHandler<GetReservationByIdQuery, ReservationResponse>
    {
        public async Task<ReservationResponse> Handle(
            GetReservationByIdQuery q,
            CancellationToken cancellationToken
        ) =>
            Map(
                await repository.GetReservationAsync(q.Id, cancellationToken)
                    ?? throw new NotFoundException("Reservation was not found.")
            );
    }
}
