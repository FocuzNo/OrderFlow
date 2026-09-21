namespace OrderFlow.Payments.Application.Abstractions.Messaging;

public interface IQuery<out TResponse> : IRequest<TResponse>;
