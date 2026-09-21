using OrderFlow.Catalog.Application.Abstractions.Messaging;

namespace OrderFlow.Catalog.Application.Products.Create;

public sealed record CreateProductCommand(string Name, string? Description, decimal Price)
    : ICommand<CreateProductResponse>;
