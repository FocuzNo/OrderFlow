using OrderFlow.Catalog.Domain.Products;

namespace OrderFlow.Catalog.Application.Products;

public static partial class ProductFeatures
{
    public sealed class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
    {
        public UpdateProductCommandValidator()
        {
            RuleFor(candidate => candidate.Id).NotEmpty();
            RuleFor(candidate => candidate.Name).NotEmpty().MaximumLength(Product.MaxNameLength);
            RuleFor(candidate => candidate.Description).MaximumLength(Product.MaxDescriptionLength);
            RuleFor(candidate => candidate.CategoryId).NotEmpty();
        }
    }
}
