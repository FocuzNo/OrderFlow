using FluentValidation;
using MediatR;
using OrderFlow.Catalog.Application.Abstractions.Errors;
using OrderFlow.Catalog.Application.Abstractions.Messaging;
using OrderFlow.Catalog.Application.Abstractions.Persistence;
using OrderFlow.Catalog.Domain.Products;

namespace OrderFlow.Catalog.Application.Products;

public static partial class ProductFeatures
{
    private static async Task<Product> Find(IProductRepository r, Guid id, CancellationToken ct) =>
        await r.GetByIdAsync(id, ct) ?? throw new NotFoundException("Product was not found.");
}
