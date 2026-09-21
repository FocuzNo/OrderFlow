using FastEndpoints;
using MediatR;
using F = OrderFlow.Ordering.Application.Orders.OrderFeatures;

namespace OrderFlow.Ordering.Api.Endpoints;

public static partial class OrderEndpoints
{
    public sealed class CancelOrderRequest
    {
        public Guid Id { get; set; }

        public string Reason { get; set; } = string.Empty;
    }
}
