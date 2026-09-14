using FastEndpoints;

namespace OrderFlow.Catalog.Api.Endpoints;

public sealed class HealthEndpoint : EndpointWithoutRequest
{
    public override void Configure()
    {
        Get("/health");
        AllowAnonymous();
    }

    public override Task HandleAsync(CancellationToken cancellationToken) =>
        Send.OkAsync(new { Status = "Healthy" }, cancellationToken);
}
