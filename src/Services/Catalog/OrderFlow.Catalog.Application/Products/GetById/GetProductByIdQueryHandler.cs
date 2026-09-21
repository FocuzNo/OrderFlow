using OrderFlow.Catalog.Application.Abstractions.Errors;
using OrderFlow.Catalog.Application.Abstractions.Messaging;
using OrderFlow.Catalog.Application.Abstractions.Persistence;
using OrderFlow.Catalog.Domain.Products;

namespace OrderFlow.Catalog.Application.Products;

public static partial class ProductFeatures
{
    public sealed class GetProductByIdQueryHandler(IProductRepository repository)
        : IRequestHandler<GetProductByIdQuery, ProductResponse>
    {
        public async Task<ProductResponse> Handle(
            GetProductByIdQuery q,
            CancellationToken cancellationToken
        ) =>
            ProductResponse.From(
                await repository.GetByIdAsync(q.Id, cancellationToken)
                    ?? throw new NotFoundException("Product was not found.")
            );
    }
}
