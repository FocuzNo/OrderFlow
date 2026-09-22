using OrderFlow.Catalog.Domain.Categories;

namespace OrderFlow.Catalog.Application.Categories;

public static partial class CategoryFeatures
{
    public sealed class UpdateCategoryCommandValidator : AbstractValidator<UpdateCategoryCommand>
    {
        public UpdateCategoryCommandValidator()
        {
            RuleFor(candidate => candidate.Id).NotEmpty();
            RuleFor(candidate => candidate.Name).NotEmpty().MaximumLength(Category.MaxNameLength);
            RuleFor(candidate => candidate.Description)
                .MaximumLength(Category.MaxDescriptionLength);
        }
    }
}
