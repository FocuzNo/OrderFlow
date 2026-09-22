namespace OrderFlow.Ordering.Application.Abstractions.Messaging;

public interface IQuery<out TResponse> : IRequest<TResponse>;
