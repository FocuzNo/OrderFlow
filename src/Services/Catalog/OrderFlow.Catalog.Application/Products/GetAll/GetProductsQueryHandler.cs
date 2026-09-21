using FluentValidation;
using MediatR;
using OrderFlow.Catalog.Application.Abstractions.Errors;
using OrderFlow.Catalog.Application.Abstractions.Messaging;
using OrderFlow.Catalog.Application.Abstractions.Persistence;
using OrderFlow.Catalog.Domain.Products;

namespace OrderFlow.Catalog.Application.Products;

public static partial class ProductFeatures
{
    public sealed class GetProductsQueryHandler(IProductRepository repository)
        : IRequestHandler<GetProductsQuery, IReadOnlyList<ProductResponse>>
    {
        public async Task<IReadOnlyList<ProductResponse>> Handle(
            GetProductsQuery q,
            CancellationToken ct
        ) =>
            (await repository.ListAsync(q.Page, q.PageSize, q.Search, q.Sort, ct))
                .Select(ProductResponse.From)
                .ToArray();
    }
}
