using FastEndpoints;
using MediatR;
using F = OrderFlow.Notifications.Application.Notifications.NotificationFeatures;

namespace OrderFlow.Notifications.Api.Endpoints;

public static class NotificationEndpoints
{
    public sealed class IdReq
    {
        public Guid Id { get; set; }
    }

    public sealed class CreateReq
    {
        public string Recipient { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public string Channel { get; set; } = "Email";
    }

    public sealed class RecipientReq
    {
        public string Recipient { get; set; } = string.Empty;
    }

    public sealed class Create(ISender s) : Endpoint<CreateReq, F.Dto>
    {
        public override void Configure()
        {
            Post("/api/notifications");
            AllowAnonymous();
        }

        public override async Task HandleAsync(CreateReq r, CancellationToken ct) =>
            await Send.ResponseAsync(
                await s.Send(new F.Create(r.Recipient, r.Subject, r.Body, r.Channel), ct),
                201,
                ct
            );
    }

    public sealed class SendNotification(ISender s) : Endpoint<IdReq, F.Dto>
    {
        public override void Configure()
        {
            Post("/api/notifications/{id}/send");
            AllowAnonymous();
        }

        public override async Task HandleAsync(IdReq r, CancellationToken ct) =>
            await Send.OkAsync(await s.Send(new F.Send(r.Id), ct), ct);
    }

    public sealed class Retry(ISender s) : Endpoint<IdReq, F.Dto>
    {
        public override void Configure()
        {
            Post("/api/notifications/{id}/retry");
            AllowAnonymous();
        }

        public override async Task HandleAsync(IdReq r, CancellationToken ct) =>
            await Send.OkAsync(await s.Send(new F.Retry(r.Id), ct), ct);
    }

    public sealed class Get(ISender s) : Endpoint<IdReq, F.Dto>
    {
        public override void Configure()
        {
            Get("/api/notifications/{id}");
            AllowAnonymous();
        }

        public override async Task HandleAsync(IdReq r, CancellationToken ct) =>
            await Send.OkAsync(await s.Send(new F.Get(r.Id), ct), ct);
    }

    public sealed class Recipient(ISender s) : Endpoint<RecipientReq, IReadOnlyList<F.Dto>>
    {
        public override void Configure()
        {
            Get("/api/notifications");
            AllowAnonymous();
        }

        public override async Task HandleAsync(RecipientReq r, CancellationToken ct) =>
            await Send.OkAsync(await s.Send(new F.GetForRecipient(r.Recipient), ct), ct);
    }
}
