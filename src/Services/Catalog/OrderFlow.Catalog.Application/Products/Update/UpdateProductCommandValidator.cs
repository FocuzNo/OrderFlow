using OrderFlow.Catalog.Domain.Products;

namespace OrderFlow.Catalog.Application.Products;

public static partial class ProductFeatures
{
    public sealed class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
    {
        public UpdateProductCommandValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.Name).NotEmpty().MaximumLength(Product.MaxNameLength);
            RuleFor(x => x.Description).MaximumLength(Product.MaxDescriptionLength);
            RuleFor(x => x.CategoryId).NotEmpty();
        }
    }
}
