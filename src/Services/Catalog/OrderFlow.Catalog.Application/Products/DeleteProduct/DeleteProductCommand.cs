using OrderFlow.Catalog.Application.Abstractions.Messaging;

namespace OrderFlow.Catalog.Application.Products;

public static partial class ProductFeatures
{
    public sealed record DeleteProductCommand(Guid Id) : ICommand;
}
