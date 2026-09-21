using MediatR;
namespace OrderFlow.Catalog.Application.Abstractions.Messaging;
public interface IQuery<out TResponse> : IRequest<TResponse>;
