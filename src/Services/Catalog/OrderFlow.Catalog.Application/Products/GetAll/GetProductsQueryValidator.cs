using OrderFlow.Catalog.Application.Abstractions.Errors;
using OrderFlow.Catalog.Application.Abstractions.Messaging;
using OrderFlow.Catalog.Application.Abstractions.Persistence;
using OrderFlow.Catalog.Domain.Products;

namespace OrderFlow.Catalog.Application.Products;

public static partial class ProductFeatures
{
    public sealed class GetProductsQueryValidator : AbstractValidator<GetProductsQuery>
    {
        public GetProductsQueryValidator()
        {
            RuleFor(candidate => candidate.Page).GreaterThan(0);
            RuleFor(candidate => candidate.PageSize).InclusiveBetween(1, 100);
        }
    }
}
