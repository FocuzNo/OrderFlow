using FluentValidation;
using MediatR;
using OrderFlow.Catalog.Application.Abstractions.Errors;
using OrderFlow.Catalog.Application.Abstractions.Messaging;
using OrderFlow.Catalog.Application.Abstractions.Persistence;
using OrderFlow.Catalog.Domain.Products;

namespace OrderFlow.Catalog.Application.Products;

public static partial class ProductFeatures
{
    public sealed class CreateProductCommandHandler(
        IProductRepository products,
        ICategoryRepository categories
    ) : IRequestHandler<CreateProductCommand, ProductResponse>
    {
        public async Task<ProductResponse> Handle(CreateProductCommand c, CancellationToken ct)
        {
            if (await products.SkuExistsAsync(c.Sku, null, ct))
                throw new ConflictException("SKU already exists.");
            if (await categories.GetByIdAsync(c.CategoryId, ct) is null)
                throw new NotFoundException("Category was not found.");
            var product = Product.Create(c.Sku, c.Name, c.Description, c.Price, c.CategoryId);
            await products.AddAsync(product, ct);
            return ProductResponse.From(product);
        }
    }
}
