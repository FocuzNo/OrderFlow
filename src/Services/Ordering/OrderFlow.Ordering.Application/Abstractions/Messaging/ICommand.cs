using MediatR;

namespace OrderFlow.Ordering.Application.Abstractions.Messaging;

public interface ICommand : IRequest;

public interface ICommand<out TResponse> : IRequest<TResponse>;
