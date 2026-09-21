using MediatR;
using OrderFlow.Inventory.Application.Abstractions.Errors;
using OrderFlow.Inventory.Application.Abstractions.Messaging;
using OrderFlow.Inventory.Application.Abstractions.Persistence;
using OrderFlow.Inventory.Domain.Stock;
using OrderFlow.Inventory.Domain.Warehouses;

namespace OrderFlow.Inventory.Application.Inventory;

public static partial class InventoryFeatures
{
    public sealed class GetReservationByIdQueryHandler(IInventoryRepository r)
        : IRequestHandler<GetReservationByIdQuery, ReservationResponse>
    {
        public async Task<ReservationResponse> Handle(
            GetReservationByIdQuery q,
            CancellationToken ct
        ) =>
            Map(
                await r.GetReservationAsync(q.Id, ct)
                    ?? throw new NotFoundException("Reservation was not found.")
            );
    }
}
