using OrderFlow.Inventory.Application.Inventory;

namespace OrderFlow.Inventory.Application.Abstractions.Messaging;

public interface IInventoryOutboxWriter
{
    void AddReserved(
        Guid orderId,
        IReadOnlyCollection<ReservedStockItem> items
    );

    void AddReservationFailed(
        Guid orderId,
        string reason
    );
}
