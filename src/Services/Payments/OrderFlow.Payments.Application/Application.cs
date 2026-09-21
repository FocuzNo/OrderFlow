using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using OrderFlow.Payments.Domain;

namespace OrderFlow.Payments.Application;

public interface IPaymentRepository
{
    Task<Payment?> GetAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<Payment>> ListAsync(CancellationToken ct);
    Task AddAsync(Payment entity, CancellationToken ct);
    Task SaveAsync(CancellationToken ct);
    Task DeleteAsync(Payment entity, CancellationToken ct);
}

public interface ICommand : IRequest;
public interface ICommand<out T> : IRequest<T>;
public interface IQuery<out T> : IRequest<T>;
public sealed record PaymentDto(Guid Id, string Reference, string? Description, decimal Value, string Status, DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt)
{
    public static PaymentDto From(Payment entity) => new(entity.Id, entity.Reference, entity.Description, entity.Value, entity.Status, entity.CreatedAt, entity.UpdatedAt);
}
public sealed record CreatePaymentCommand(string Reference, string? Description, decimal Value) : ICommand<PaymentDto>;
public sealed record GetPaymentQuery(Guid Id) : IQuery<PaymentDto>;
public sealed record ListPaymentsQuery : IQuery<IReadOnlyList<PaymentDto>>;
public sealed record UpdatePaymentCommand(Guid Id, string Reference, string? Description, decimal Value, string Status) : ICommand<PaymentDto>;
public sealed record DeletePaymentCommand(Guid Id) : ICommand;

public sealed class CreateValidator : AbstractValidator<CreatePaymentCommand>
{
    public CreateValidator() { RuleFor(x => x.Reference).NotEmpty().MaximumLength(Payment.MaxReferenceLength); RuleFor(x => x.Description).MaximumLength(Payment.MaxDescriptionLength); RuleFor(x => x.Value).GreaterThanOrEqualTo(0); }
}
public sealed class UpdateValidator : AbstractValidator<UpdatePaymentCommand>
{
    public UpdateValidator() { RuleFor(x => x.Id).NotEmpty(); RuleFor(x => x.Reference).NotEmpty().MaximumLength(Payment.MaxReferenceLength); RuleFor(x => x.Description).MaximumLength(Payment.MaxDescriptionLength); RuleFor(x => x.Value).GreaterThanOrEqualTo(0); RuleFor(x => x.Status).NotEmpty().MaximumLength(50); }
}
public sealed class CreateHandler(IPaymentRepository repository) : IRequestHandler<CreatePaymentCommand, PaymentDto>
{
    public async Task<PaymentDto> Handle(CreatePaymentCommand command, CancellationToken ct) { var entity = Payment.Create(command.Reference, command.Description, command.Value); await repository.AddAsync(entity, ct); return PaymentDto.From(entity); }
}
public sealed class GetHandler(IPaymentRepository repository) : IRequestHandler<GetPaymentQuery, PaymentDto>
{
    public async Task<PaymentDto> Handle(GetPaymentQuery query, CancellationToken ct) => PaymentDto.From(await repository.GetAsync(query.Id, ct) ?? throw new KeyNotFoundException($"Payment '{query.Id}' was not found."));
}
public sealed class ListHandler(IPaymentRepository repository) : IRequestHandler<ListPaymentsQuery, IReadOnlyList<PaymentDto>>
{
    public async Task<IReadOnlyList<PaymentDto>> Handle(ListPaymentsQuery query, CancellationToken ct) => (await repository.ListAsync(ct)).Select(PaymentDto.From).ToArray();
}
public sealed class UpdateHandler(IPaymentRepository repository) : IRequestHandler<UpdatePaymentCommand, PaymentDto>
{
    public async Task<PaymentDto> Handle(UpdatePaymentCommand command, CancellationToken ct) { var entity = await repository.GetAsync(command.Id, ct) ?? throw new KeyNotFoundException($"Payment '{command.Id}' was not found."); entity.Update(command.Reference, command.Description, command.Value, command.Status); await repository.SaveAsync(ct); return PaymentDto.From(entity); }
}
public sealed class DeleteHandler(IPaymentRepository repository) : IRequestHandler<DeletePaymentCommand>
{
    public async Task Handle(DeletePaymentCommand command, CancellationToken ct) { var entity = await repository.GetAsync(command.Id, ct) ?? throw new KeyNotFoundException($"Payment '{command.Id}' was not found."); entity.MarkDeleted(); await repository.DeleteAsync(entity, ct); }
}
public sealed class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators) : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct) { var failures = validators.Select(v => v.Validate(request)).SelectMany(r => r.Errors).Where(e => e is not null).ToArray(); if (failures.Length > 0) throw new ValidationException(failures); return await next(); }
}
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services) { var assembly = typeof(DependencyInjection).Assembly; services.AddMediatR(c => { c.RegisterServicesFromAssembly(assembly); c.AddOpenBehavior(typeof(ValidationBehavior<,>)); }); services.AddValidatorsFromAssembly(assembly); return services; }
}
