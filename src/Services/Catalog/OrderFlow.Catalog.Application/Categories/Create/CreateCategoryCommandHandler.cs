using MediatR;
using OrderFlow.Catalog.Application.Abstractions.Errors;
using OrderFlow.Catalog.Application.Abstractions.Messaging;
using OrderFlow.Catalog.Application.Abstractions.Persistence;
using OrderFlow.Catalog.Domain.Categories;

namespace OrderFlow.Catalog.Application.Categories;

public static partial class CategoryFeatures
{
    public sealed class CreateCategoryCommandHandler(ICategoryRepository r)
        : IRequestHandler<CreateCategoryCommand, CategoryResponse>
    {
        public async Task<CategoryResponse> Handle(CreateCategoryCommand c, CancellationToken ct)
        {
            if (await r.NameExistsAsync(c.Name, null, ct))
                throw new ConflictException("Category name already exists.");
            var entity = Category.Create(c.Name, c.Description);
            await r.AddAsync(entity, ct);
            return CategoryResponse.From(entity);
        }
    }
}
