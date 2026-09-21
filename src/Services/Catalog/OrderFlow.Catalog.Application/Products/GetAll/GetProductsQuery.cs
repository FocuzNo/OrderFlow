using FluentValidation;
using MediatR;
using OrderFlow.Catalog.Application.Abstractions.Errors;
using OrderFlow.Catalog.Application.Abstractions.Messaging;
using OrderFlow.Catalog.Application.Abstractions.Persistence;
using OrderFlow.Catalog.Domain.Products;

namespace OrderFlow.Catalog.Application.Products;

public static partial class ProductFeatures
{
    public sealed record GetProductsQuery(
        int Page = 1,
        int PageSize = 20,
        string? Search = null,
        string? Sort = null
    ) : IQuery<IReadOnlyList<ProductResponse>>;
}
