using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using OrderFlow.Notifications.Domain;

namespace OrderFlow.Notifications.Application;

public interface INotificationRepository
{
    Task<Notification?> GetAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<Notification>> ListAsync(CancellationToken ct);
    Task AddAsync(Notification entity, CancellationToken ct);
    Task SaveAsync(CancellationToken ct);
    Task DeleteAsync(Notification entity, CancellationToken ct);
}

public interface ICommand : IRequest;
public interface ICommand<out T> : IRequest<T>;
public interface IQuery<out T> : IRequest<T>;
public sealed record NotificationDto(Guid Id, string Reference, string? Description, decimal Value, string Status, DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt)
{
    public static NotificationDto From(Notification entity) => new(entity.Id, entity.Reference, entity.Description, entity.Value, entity.Status, entity.CreatedAt, entity.UpdatedAt);
}
public sealed record CreateNotificationCommand(string Reference, string? Description, decimal Value) : ICommand<NotificationDto>;
public sealed record GetNotificationQuery(Guid Id) : IQuery<NotificationDto>;
public sealed record ListNotificationsQuery : IQuery<IReadOnlyList<NotificationDto>>;
public sealed record UpdateNotificationCommand(Guid Id, string Reference, string? Description, decimal Value, string Status) : ICommand<NotificationDto>;
public sealed record DeleteNotificationCommand(Guid Id) : ICommand;

public sealed class CreateValidator : AbstractValidator<CreateNotificationCommand>
{
    public CreateValidator() { RuleFor(x => x.Reference).NotEmpty().MaximumLength(Notification.MaxReferenceLength); RuleFor(x => x.Description).MaximumLength(Notification.MaxDescriptionLength); RuleFor(x => x.Value).GreaterThanOrEqualTo(0); }
}
public sealed class UpdateValidator : AbstractValidator<UpdateNotificationCommand>
{
    public UpdateValidator() { RuleFor(x => x.Id).NotEmpty(); RuleFor(x => x.Reference).NotEmpty().MaximumLength(Notification.MaxReferenceLength); RuleFor(x => x.Description).MaximumLength(Notification.MaxDescriptionLength); RuleFor(x => x.Value).GreaterThanOrEqualTo(0); RuleFor(x => x.Status).NotEmpty().MaximumLength(50); }
}
public sealed class CreateHandler(INotificationRepository repository) : IRequestHandler<CreateNotificationCommand, NotificationDto>
{
    public async Task<NotificationDto> Handle(CreateNotificationCommand command, CancellationToken ct) { var entity = Notification.Create(command.Reference, command.Description, command.Value); await repository.AddAsync(entity, ct); return NotificationDto.From(entity); }
}
public sealed class GetHandler(INotificationRepository repository) : IRequestHandler<GetNotificationQuery, NotificationDto>
{
    public async Task<NotificationDto> Handle(GetNotificationQuery query, CancellationToken ct) => NotificationDto.From(await repository.GetAsync(query.Id, ct) ?? throw new KeyNotFoundException($"Notification '{query.Id}' was not found."));
}
public sealed class ListHandler(INotificationRepository repository) : IRequestHandler<ListNotificationsQuery, IReadOnlyList<NotificationDto>>
{
    public async Task<IReadOnlyList<NotificationDto>> Handle(ListNotificationsQuery query, CancellationToken ct) => (await repository.ListAsync(ct)).Select(NotificationDto.From).ToArray();
}
public sealed class UpdateHandler(INotificationRepository repository) : IRequestHandler<UpdateNotificationCommand, NotificationDto>
{
    public async Task<NotificationDto> Handle(UpdateNotificationCommand command, CancellationToken ct) { var entity = await repository.GetAsync(command.Id, ct) ?? throw new KeyNotFoundException($"Notification '{command.Id}' was not found."); entity.Update(command.Reference, command.Description, command.Value, command.Status); await repository.SaveAsync(ct); return NotificationDto.From(entity); }
}
public sealed class DeleteHandler(INotificationRepository repository) : IRequestHandler<DeleteNotificationCommand>
{
    public async Task Handle(DeleteNotificationCommand command, CancellationToken ct) { var entity = await repository.GetAsync(command.Id, ct) ?? throw new KeyNotFoundException($"Notification '{command.Id}' was not found."); entity.MarkDeleted(); await repository.DeleteAsync(entity, ct); }
}
public sealed class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators) : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct) { var failures = validators.Select(v => v.Validate(request)).SelectMany(r => r.Errors).Where(e => e is not null).ToArray(); if (failures.Length > 0) throw new ValidationException(failures); return await next(); }
}
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services) { var assembly = typeof(DependencyInjection).Assembly; services.AddMediatR(c => { c.RegisterServicesFromAssembly(assembly); c.AddOpenBehavior(typeof(ValidationBehavior<,>)); }); services.AddValidatorsFromAssembly(assembly); return services; }
}
