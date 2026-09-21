namespace OrderFlow.Catalog.Application.Products.Create;

public sealed record CreateProductResponse(
    Guid Id,
    string Name,
    string? Description,
    decimal Price,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
