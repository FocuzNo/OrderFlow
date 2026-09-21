using OrderFlow.Catalog.Application.Abstractions.Errors;
using OrderFlow.Catalog.Application.Abstractions.Messaging;
using OrderFlow.Catalog.Application.Abstractions.Persistence;
using OrderFlow.Catalog.Domain.Categories;

namespace OrderFlow.Catalog.Application.Categories;

public static partial class CategoryFeatures
{
    public sealed class GetCategoriesQueryHandler(ICategoryRepository repository)
        : IRequestHandler<GetCategoriesQuery, IReadOnlyList<CategoryResponse>>
    {
        public async Task<IReadOnlyList<CategoryResponse>> Handle(
            GetCategoriesQuery q,
            CancellationToken cancellationToken
        ) =>
            (await repository.ListAsync(cancellationToken)).Select(CategoryResponse.From).ToArray();
    }
}
