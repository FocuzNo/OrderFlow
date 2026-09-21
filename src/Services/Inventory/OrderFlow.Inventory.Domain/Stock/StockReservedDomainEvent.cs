using OrderFlow.Inventory.Domain.Common;

namespace OrderFlow.Inventory.Domain.Stock;

public sealed record StockReservedDomainEvent(
    Guid EventId,
    DateTimeOffset OccurredOnUtc,
    Guid ReservationId,
    Guid OrderId,
    Guid ProductId,
    int Quantity
) : IDomainEvent;
