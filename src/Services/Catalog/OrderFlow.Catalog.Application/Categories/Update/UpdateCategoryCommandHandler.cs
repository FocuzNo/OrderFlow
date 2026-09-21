using OrderFlow.Catalog.Application.Abstractions.Errors;
using OrderFlow.Catalog.Application.Abstractions.Messaging;
using OrderFlow.Catalog.Application.Abstractions.Persistence;
using OrderFlow.Catalog.Domain.Categories;

namespace OrderFlow.Catalog.Application.Categories;

public static partial class CategoryFeatures
{
    public sealed class UpdateCategoryCommandHandler(
        ICategoryRepository repository,
        IUnitOfWork unitOfWork
    ) : IRequestHandler<UpdateCategoryCommand, CategoryResponse>
    {
        public async Task<CategoryResponse> Handle(
            UpdateCategoryCommand command,
            CancellationToken cancellationToken
        )
        {
            var entity =
                await repository.GetByIdAsync(command.Id, cancellationToken)
                ?? throw new NotFoundException("Category was not found.");
            if (await repository.NameExistsAsync(command.Name, command.Id, cancellationToken))
                throw new ConflictException("Category name already exists.");
            entity.Update(command.Name, command.Description);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return CategoryResponse.From(entity);
        }
    }
}
