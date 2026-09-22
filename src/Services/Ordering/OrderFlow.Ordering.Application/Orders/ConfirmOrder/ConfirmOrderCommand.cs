using OrderFlow.Ordering.Application.Abstractions.Messaging;

namespace OrderFlow.Ordering.Application.Orders;

public static partial class OrderFeatures
{
    public sealed record ConfirmOrderCommand(Guid OrderId) : ICommand;
}
