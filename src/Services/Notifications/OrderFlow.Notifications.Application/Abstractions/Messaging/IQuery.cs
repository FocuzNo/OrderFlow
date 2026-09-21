using MediatR;
namespace OrderFlow.Notifications.Application.Abstractions.Messaging;
public interface IQuery<out TResponse> : IRequest<TResponse>;
