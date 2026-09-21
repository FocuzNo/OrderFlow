using FastEndpoints;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Diagnostics;
using OrderFlow.Payments.Application;
using OrderFlow.Payments.Infrastructure;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSerilog(c => c.Enrich.FromLogContext().WriteTo.Console());
builder.Services.AddApplication().AddInfrastructure(builder.Configuration);
builder.Services.AddFastEndpoints(); builder.Services.AddOpenApi(); builder.Services.AddProblemDetails(); builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddHealthChecks().AddDbContextCheck<PaymentsDbContext>();
var app = builder.Build(); app.UseSerilogRequestLogging(); app.UseExceptionHandler(); app.UseFastEndpoints(); app.MapOpenApi(); app.MapHealthChecks("/health"); app.Run();

public sealed class ApiExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken ct)
    {
        if (exception is ValidationException validation) { var errors = validation.Errors.GroupBy(x => x.PropertyName).ToDictionary(x => x.Key, x => x.Select(e => e.ErrorMessage).ToArray()); await Results.ValidationProblem(errors).ExecuteAsync(context); return true; }
        if (exception is KeyNotFoundException) { await Results.Problem(statusCode: 404, title: "Resource not found", detail: exception.Message).ExecuteAsync(context); return true; }
        if (exception is ArgumentException or InvalidOperationException) { await Results.Problem(statusCode: 400, title: "Business rule violation", detail: exception.Message).ExecuteAsync(context); return true; }
        return false;
    }
}
public sealed class IdRequest { public Guid Id { get; set; } }
public class CreateRequest { public string Reference { get; set; } = string.Empty; public string? Description { get; set; } public decimal Value { get; set; } }
public sealed class UpdateRequest : CreateRequest { public Guid Id { get; set; } public string Status { get; set; } = string.Empty; }
public sealed class CreateEndpoint(ISender sender) : Endpoint<CreateRequest, PaymentDto>
{
    public override void Configure() { Post("/api/payments"); AllowAnonymous(); }
    public override async Task HandleAsync(CreateRequest r, CancellationToken ct) => await Send.ResponseAsync(await sender.Send(new CreatePaymentCommand(r.Reference, r.Description, r.Value), ct), 201, ct);
}
public sealed class GetEndpoint(ISender sender) : Endpoint<IdRequest, PaymentDto>
{
    public override void Configure() { Get("/api/payments/{id}"); AllowAnonymous(); }
    public override async Task HandleAsync(IdRequest r, CancellationToken ct) => await Send.OkAsync(await sender.Send(new GetPaymentQuery(r.Id), ct), ct);
}
public sealed class ListEndpoint(ISender sender) : EndpointWithoutRequest<IReadOnlyList<PaymentDto>>
{
    public override void Configure() { Get("/api/payments"); AllowAnonymous(); }
    public override async Task HandleAsync(CancellationToken ct) => await Send.OkAsync(await sender.Send(new ListPaymentsQuery(), ct), ct);
}
public sealed class UpdateEndpoint(ISender sender) : Endpoint<UpdateRequest, PaymentDto>
{
    public override void Configure() { Put("/api/payments/{id}"); AllowAnonymous(); }
    public override async Task HandleAsync(UpdateRequest r, CancellationToken ct) => await Send.OkAsync(await sender.Send(new UpdatePaymentCommand(r.Id, r.Reference, r.Description, r.Value, r.Status), ct), ct);
}
public sealed class DeleteEndpoint(ISender sender) : Endpoint<IdRequest>
{
    public override void Configure() { Delete("/api/payments/{id}"); AllowAnonymous(); }
    public override async Task HandleAsync(IdRequest r, CancellationToken ct) { await sender.Send(new DeletePaymentCommand(r.Id), ct); await Send.NoContentAsync(ct); }
}
