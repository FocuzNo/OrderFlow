using FastEndpoints;
using MediatR;
using F = OrderFlow.Ordering.Application.Orders.OrderFeatures;

namespace OrderFlow.Ordering.Api.Endpoints;

public static partial class OrderEndpoints
{
    public sealed class RemoveOrderItemRequest
    {
        public Guid Id { get; set; }

        public Guid ItemId { get; set; }
    }
}
