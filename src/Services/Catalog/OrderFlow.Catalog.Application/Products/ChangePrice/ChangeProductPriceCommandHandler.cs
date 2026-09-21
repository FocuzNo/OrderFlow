using FluentValidation;
using MediatR;
using OrderFlow.Catalog.Application.Abstractions.Errors;
using OrderFlow.Catalog.Application.Abstractions.Messaging;
using OrderFlow.Catalog.Application.Abstractions.Persistence;
using OrderFlow.Catalog.Domain.Products;

namespace OrderFlow.Catalog.Application.Products;

public static partial class ProductFeatures
{
    public sealed class ChangeProductPriceCommandHandler(IProductRepository repository)
        : IRequestHandler<ChangeProductPriceCommand, ProductResponse>
    {
        public async Task<ProductResponse> Handle(ChangeProductPriceCommand c, CancellationToken ct)
        {
            var p = await Find(repository, c.Id, ct);
            p.ChangePrice(c.Price);
            await repository.SaveAsync(ct);
            return ProductResponse.From(p);
        }
    }
}
