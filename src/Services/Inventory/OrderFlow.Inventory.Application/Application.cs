using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using OrderFlow.Inventory.Domain;

namespace OrderFlow.Inventory.Application;

public interface IStockItemRepository
{
    Task<StockItem?> GetAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<StockItem>> ListAsync(CancellationToken ct);
    Task AddAsync(StockItem entity, CancellationToken ct);
    Task SaveAsync(CancellationToken ct);
    Task DeleteAsync(StockItem entity, CancellationToken ct);
}

public interface ICommand : IRequest;
public interface ICommand<out T> : IRequest<T>;
public interface IQuery<out T> : IRequest<T>;
public sealed record StockItemDto(Guid Id, string Reference, string? Description, decimal Value, string Status, DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt)
{
    public static StockItemDto From(StockItem entity) => new(entity.Id, entity.Reference, entity.Description, entity.Value, entity.Status, entity.CreatedAt, entity.UpdatedAt);
}
public sealed record CreateStockItemCommand(string Reference, string? Description, decimal Value) : ICommand<StockItemDto>;
public sealed record GetStockItemQuery(Guid Id) : IQuery<StockItemDto>;
public sealed record ListStockItemsQuery : IQuery<IReadOnlyList<StockItemDto>>;
public sealed record UpdateStockItemCommand(Guid Id, string Reference, string? Description, decimal Value, string Status) : ICommand<StockItemDto>;
public sealed record DeleteStockItemCommand(Guid Id) : ICommand;

public sealed class CreateValidator : AbstractValidator<CreateStockItemCommand>
{
    public CreateValidator() { RuleFor(x => x.Reference).NotEmpty().MaximumLength(StockItem.MaxReferenceLength); RuleFor(x => x.Description).MaximumLength(StockItem.MaxDescriptionLength); RuleFor(x => x.Value).GreaterThanOrEqualTo(0); }
}
public sealed class UpdateValidator : AbstractValidator<UpdateStockItemCommand>
{
    public UpdateValidator() { RuleFor(x => x.Id).NotEmpty(); RuleFor(x => x.Reference).NotEmpty().MaximumLength(StockItem.MaxReferenceLength); RuleFor(x => x.Description).MaximumLength(StockItem.MaxDescriptionLength); RuleFor(x => x.Value).GreaterThanOrEqualTo(0); RuleFor(x => x.Status).NotEmpty().MaximumLength(50); }
}
public sealed class CreateHandler(IStockItemRepository repository) : IRequestHandler<CreateStockItemCommand, StockItemDto>
{
    public async Task<StockItemDto> Handle(CreateStockItemCommand command, CancellationToken ct) { var entity = StockItem.Create(command.Reference, command.Description, command.Value); await repository.AddAsync(entity, ct); return StockItemDto.From(entity); }
}
public sealed class GetHandler(IStockItemRepository repository) : IRequestHandler<GetStockItemQuery, StockItemDto>
{
    public async Task<StockItemDto> Handle(GetStockItemQuery query, CancellationToken ct) => StockItemDto.From(await repository.GetAsync(query.Id, ct) ?? throw new KeyNotFoundException($"StockItem '{query.Id}' was not found."));
}
public sealed class ListHandler(IStockItemRepository repository) : IRequestHandler<ListStockItemsQuery, IReadOnlyList<StockItemDto>>
{
    public async Task<IReadOnlyList<StockItemDto>> Handle(ListStockItemsQuery query, CancellationToken ct) => (await repository.ListAsync(ct)).Select(StockItemDto.From).ToArray();
}
public sealed class UpdateHandler(IStockItemRepository repository) : IRequestHandler<UpdateStockItemCommand, StockItemDto>
{
    public async Task<StockItemDto> Handle(UpdateStockItemCommand command, CancellationToken ct) { var entity = await repository.GetAsync(command.Id, ct) ?? throw new KeyNotFoundException($"StockItem '{command.Id}' was not found."); entity.Update(command.Reference, command.Description, command.Value, command.Status); await repository.SaveAsync(ct); return StockItemDto.From(entity); }
}
public sealed class DeleteHandler(IStockItemRepository repository) : IRequestHandler<DeleteStockItemCommand>
{
    public async Task Handle(DeleteStockItemCommand command, CancellationToken ct) { var entity = await repository.GetAsync(command.Id, ct) ?? throw new KeyNotFoundException($"StockItem '{command.Id}' was not found."); entity.MarkDeleted(); await repository.DeleteAsync(entity, ct); }
}
public sealed class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators) : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct) { var failures = validators.Select(v => v.Validate(request)).SelectMany(r => r.Errors).Where(e => e is not null).ToArray(); if (failures.Length > 0) throw new ValidationException(failures); return await next(); }
}
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services) { var assembly = typeof(DependencyInjection).Assembly; services.AddMediatR(c => { c.RegisterServicesFromAssembly(assembly); c.AddOpenBehavior(typeof(ValidationBehavior<,>)); }); services.AddValidatorsFromAssembly(assembly); return services; }
}
