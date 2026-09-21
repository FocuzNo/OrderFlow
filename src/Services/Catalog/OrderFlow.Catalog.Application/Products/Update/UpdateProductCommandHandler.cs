using FluentValidation;
using MediatR;
using OrderFlow.Catalog.Application.Abstractions.Errors;
using OrderFlow.Catalog.Application.Abstractions.Messaging;
using OrderFlow.Catalog.Application.Abstractions.Persistence;
using OrderFlow.Catalog.Domain.Products;

namespace OrderFlow.Catalog.Application.Products;

public static partial class ProductFeatures
{
    public sealed class UpdateProductCommandHandler(
        IProductRepository products,
        ICategoryRepository categories
    ) : IRequestHandler<UpdateProductCommand, ProductResponse>
    {
        public async Task<ProductResponse> Handle(UpdateProductCommand c, CancellationToken ct)
        {
            var p = await Find(products, c.Id, ct);
            if (await categories.GetByIdAsync(c.CategoryId, ct) is null)
                throw new NotFoundException("Category was not found.");
            p.Update(c.Name, c.Description, c.CategoryId);
            await products.SaveAsync(ct);
            return ProductResponse.From(p);
        }
    }
}
