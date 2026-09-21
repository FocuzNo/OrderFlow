using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using OrderFlow.Ordering.Domain;

namespace OrderFlow.Ordering.Application;

public interface IOrderRepository
{
    Task<Order?> GetAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<Order>> ListAsync(CancellationToken ct);
    Task AddAsync(Order entity, CancellationToken ct);
    Task SaveAsync(CancellationToken ct);
    Task DeleteAsync(Order entity, CancellationToken ct);
}

public interface ICommand : IRequest;
public interface ICommand<out T> : IRequest<T>;
public interface IQuery<out T> : IRequest<T>;
public sealed record OrderDto(Guid Id, string Reference, string? Description, decimal Value, string Status, DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt)
{
    public static OrderDto From(Order entity) => new(entity.Id, entity.Reference, entity.Description, entity.Value, entity.Status, entity.CreatedAt, entity.UpdatedAt);
}
public sealed record CreateOrderCommand(string Reference, string? Description, decimal Value) : ICommand<OrderDto>;
public sealed record GetOrderQuery(Guid Id) : IQuery<OrderDto>;
public sealed record ListOrdersQuery : IQuery<IReadOnlyList<OrderDto>>;
public sealed record UpdateOrderCommand(Guid Id, string Reference, string? Description, decimal Value, string Status) : ICommand<OrderDto>;
public sealed record DeleteOrderCommand(Guid Id) : ICommand;

public sealed class CreateValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateValidator() { RuleFor(x => x.Reference).NotEmpty().MaximumLength(Order.MaxReferenceLength); RuleFor(x => x.Description).MaximumLength(Order.MaxDescriptionLength); RuleFor(x => x.Value).GreaterThanOrEqualTo(0); }
}
public sealed class UpdateValidator : AbstractValidator<UpdateOrderCommand>
{
    public UpdateValidator() { RuleFor(x => x.Id).NotEmpty(); RuleFor(x => x.Reference).NotEmpty().MaximumLength(Order.MaxReferenceLength); RuleFor(x => x.Description).MaximumLength(Order.MaxDescriptionLength); RuleFor(x => x.Value).GreaterThanOrEqualTo(0); RuleFor(x => x.Status).NotEmpty().MaximumLength(50); }
}
public sealed class CreateHandler(IOrderRepository repository) : IRequestHandler<CreateOrderCommand, OrderDto>
{
    public async Task<OrderDto> Handle(CreateOrderCommand command, CancellationToken ct) { var entity = Order.Create(command.Reference, command.Description, command.Value); await repository.AddAsync(entity, ct); return OrderDto.From(entity); }
}
public sealed class GetHandler(IOrderRepository repository) : IRequestHandler<GetOrderQuery, OrderDto>
{
    public async Task<OrderDto> Handle(GetOrderQuery query, CancellationToken ct) => OrderDto.From(await repository.GetAsync(query.Id, ct) ?? throw new KeyNotFoundException($"Order '{query.Id}' was not found."));
}
public sealed class ListHandler(IOrderRepository repository) : IRequestHandler<ListOrdersQuery, IReadOnlyList<OrderDto>>
{
    public async Task<IReadOnlyList<OrderDto>> Handle(ListOrdersQuery query, CancellationToken ct) => (await repository.ListAsync(ct)).Select(OrderDto.From).ToArray();
}
public sealed class UpdateHandler(IOrderRepository repository) : IRequestHandler<UpdateOrderCommand, OrderDto>
{
    public async Task<OrderDto> Handle(UpdateOrderCommand command, CancellationToken ct) { var entity = await repository.GetAsync(command.Id, ct) ?? throw new KeyNotFoundException($"Order '{command.Id}' was not found."); entity.Update(command.Reference, command.Description, command.Value, command.Status); await repository.SaveAsync(ct); return OrderDto.From(entity); }
}
public sealed class DeleteHandler(IOrderRepository repository) : IRequestHandler<DeleteOrderCommand>
{
    public async Task Handle(DeleteOrderCommand command, CancellationToken ct) { var entity = await repository.GetAsync(command.Id, ct) ?? throw new KeyNotFoundException($"Order '{command.Id}' was not found."); entity.MarkDeleted(); await repository.DeleteAsync(entity, ct); }
}
public sealed class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators) : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct) { var failures = validators.Select(v => v.Validate(request)).SelectMany(r => r.Errors).Where(e => e is not null).ToArray(); if (failures.Length > 0) throw new ValidationException(failures); return await next(); }
}
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services) { var assembly = typeof(DependencyInjection).Assembly; services.AddMediatR(c => { c.RegisterServicesFromAssembly(assembly); c.AddOpenBehavior(typeof(ValidationBehavior<,>)); }); services.AddValidatorsFromAssembly(assembly); return services; }
}
