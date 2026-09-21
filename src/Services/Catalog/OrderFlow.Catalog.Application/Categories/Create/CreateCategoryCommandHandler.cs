using OrderFlow.Catalog.Application.Abstractions.Errors;
using OrderFlow.Catalog.Application.Abstractions.Messaging;
using OrderFlow.Catalog.Application.Abstractions.Persistence;
using OrderFlow.Catalog.Domain.Categories;

namespace OrderFlow.Catalog.Application.Categories;

public static partial class CategoryFeatures
{
    public sealed class CreateCategoryCommandHandler(
        ICategoryRepository repository,
        IUnitOfWork unitOfWork
    ) : IRequestHandler<CreateCategoryCommand, CategoryResponse>
    {
        public async Task<CategoryResponse> Handle(
            CreateCategoryCommand command,
            CancellationToken cancellationToken
        )
        {
            if (await repository.NameExistsAsync(command.Name, null, cancellationToken))
                throw new ConflictException("Category name already exists.");
            var entity = Category.Create(command.Name, command.Description);
            await repository.AddAsync(entity, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return CategoryResponse.From(entity);
        }
    }
}
