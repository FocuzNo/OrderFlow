using OrderFlow.Catalog.Domain.Categories;

namespace OrderFlow.Catalog.Application.Categories;

public static partial class CategoryFeatures
{
    public sealed class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
    {
        public CreateCategoryCommandValidator()
        {
            RuleFor(candidate => candidate.Name).NotEmpty().MaximumLength(Category.MaxNameLength);
            RuleFor(candidate => candidate.Description)
                .MaximumLength(Category.MaxDescriptionLength);
        }
    }
}
