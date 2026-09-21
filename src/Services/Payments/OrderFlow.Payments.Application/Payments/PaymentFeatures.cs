using MediatR;
using OrderFlow.Payments.Application.Abstractions.Errors;
using OrderFlow.Payments.Application.Abstractions.Messaging;
using OrderFlow.Payments.Application.Abstractions.Payments;
using OrderFlow.Payments.Application.Abstractions.Persistence;
using OrderFlow.Payments.Domain.Payments;

namespace OrderFlow.Payments.Application.Payments;

public static class PaymentFeatures
{
    public sealed record RefundDto(
        Guid Id,
        decimal Amount,
        string Reason,
        DateTimeOffset CreatedAt
    );

    public sealed record Dto(
        Guid Id,
        Guid OrderId,
        decimal Amount,
        string Method,
        string Status,
        string? ProviderReference,
        string? FailureReason,
        IReadOnlyList<RefundDto> Refunds
    );

    public sealed record Create(Guid OrderId, decimal Amount, string Method) : ICommand<Dto>;

    public sealed class CreateHandler(IPaymentRepository r) : IRequestHandler<Create, Dto>
    {
        public async Task<Dto> Handle(Create c, CancellationToken ct)
        {
            if (await r.GetByOrderAsync(c.OrderId, ct) is not null)
                throw new ConflictException("Payment for order already exists.");
            var x = Payment.Create(c.OrderId, c.Amount, PaymentMethod.FromName(c.Method, true));
            await r.AddAsync(x, ct);
            return Map(x);
        }
    }

    public sealed record Process(Guid PaymentId) : ICommand<Dto>;

    public sealed class ProcessHandler(IPaymentRepository r, IPaymentGateway gateway)
        : IRequestHandler<Process, Dto>
    {
        public async Task<Dto> Handle(Process c, CancellationToken ct)
        {
            var x = await Find(r, c.PaymentId, ct);
            x.StartProcessing();
            var result = await gateway.ChargeAsync(x.Id, x.Amount, ct);
            if (result.Succeeded)
                x.Succeed(result.Reference ?? $"DEV-{x.Id:N}");
            else
                x.Fail(result.Error ?? "Payment declined.");
            await r.SaveAsync(ct);
            return Map(x);
        }
    }

    public sealed record GetById(Guid Id) : IQuery<Dto>;

    public sealed class GetByIdHandler(IPaymentRepository r) : IRequestHandler<GetById, Dto>
    {
        public async Task<Dto> Handle(GetById q, CancellationToken ct) =>
            Map(await Find(r, q.Id, ct));
    }

    public sealed record GetByOrder(Guid OrderId) : IQuery<Dto>;

    public sealed class GetByOrderHandler(IPaymentRepository r) : IRequestHandler<GetByOrder, Dto>
    {
        public async Task<Dto> Handle(GetByOrder q, CancellationToken ct) =>
            Map(
                await r.GetByOrderAsync(q.OrderId, ct)
                    ?? throw new NotFoundException("Payment was not found.")
            );
    }

    public sealed record MarkSucceeded(Guid PaymentId, string Reference) : ICommand;

    public sealed class MarkSucceededHandler(IPaymentRepository r) : IRequestHandler<MarkSucceeded>
    {
        public async Task Handle(MarkSucceeded c, CancellationToken ct)
        {
            var x = await Find(r, c.PaymentId, ct);
            x.Succeed(c.Reference);
            await r.SaveAsync(ct);
        }
    }

    public sealed record MarkFailed(Guid PaymentId, string Reason) : ICommand;

    public sealed class MarkFailedHandler(IPaymentRepository r) : IRequestHandler<MarkFailed>
    {
        public async Task Handle(MarkFailed c, CancellationToken ct)
        {
            var x = await Find(r, c.PaymentId, ct);
            x.Fail(c.Reason);
            await r.SaveAsync(ct);
        }
    }

    public sealed record RefundPayment(Guid PaymentId, decimal Amount, string Reason)
        : ICommand<Dto>;

    public sealed class RefundHandler(IPaymentRepository r) : IRequestHandler<RefundPayment, Dto>
    {
        public async Task<Dto> Handle(RefundPayment c, CancellationToken ct)
        {
            var x = await Find(r, c.PaymentId, ct);
            x.Refund(c.Amount, c.Reason);
            await r.SaveAsync(ct);
            return Map(x);
        }
    }

    private static async Task<Payment> Find(IPaymentRepository r, Guid id, CancellationToken ct) =>
        await r.GetAsync(id, ct) ?? throw new NotFoundException("Payment was not found.");

    private static Dto Map(Payment x) =>
        new(
            x.Id,
            x.OrderId,
            x.Amount,
            x.Method.Name,
            x.Status.Name,
            x.ProviderReference,
            x.FailureReason,
            x.Refunds.Select(y => new RefundDto(y.Id, y.Amount, y.Reason, y.CreatedAt)).ToArray()
        );
}
