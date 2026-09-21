using MediatR;

namespace OrderFlow.Inventory.Application.Abstractions.Messaging;

public interface IQuery<out TResponse> : IRequest<TResponse>;
