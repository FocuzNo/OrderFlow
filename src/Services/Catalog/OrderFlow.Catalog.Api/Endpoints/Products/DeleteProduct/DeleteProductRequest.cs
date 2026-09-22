namespace OrderFlow.Catalog.Api.Endpoints;

public sealed record DeleteProductRequest
{
    public Guid Id { get; init; }
}
