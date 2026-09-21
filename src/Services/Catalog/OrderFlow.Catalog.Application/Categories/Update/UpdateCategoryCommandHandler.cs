using MediatR;
using OrderFlow.Catalog.Application.Abstractions.Errors;
using OrderFlow.Catalog.Application.Abstractions.Messaging;
using OrderFlow.Catalog.Application.Abstractions.Persistence;
using OrderFlow.Catalog.Domain.Categories;

namespace OrderFlow.Catalog.Application.Categories;

public static partial class CategoryFeatures
{
    public sealed class UpdateCategoryCommandHandler(ICategoryRepository r)
        : IRequestHandler<UpdateCategoryCommand, CategoryResponse>
    {
        public async Task<CategoryResponse> Handle(UpdateCategoryCommand c, CancellationToken ct)
        {
            var x =
                await r.GetByIdAsync(c.Id, ct)
                ?? throw new NotFoundException("Category was not found.");
            if (await r.NameExistsAsync(c.Name, c.Id, ct))
                throw new ConflictException("Category name already exists.");
            x.Update(c.Name, c.Description);
            await r.SaveAsync(ct);
            return CategoryResponse.From(x);
        }
    }
}
