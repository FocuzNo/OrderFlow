using OrderFlow.Catalog.Application.Abstractions.Errors;
using OrderFlow.Catalog.Application.Abstractions.Messaging;
using OrderFlow.Catalog.Application.Abstractions.Persistence;
using OrderFlow.Catalog.Domain.Products;

namespace OrderFlow.Catalog.Application.Products;

public static partial class ProductFeatures
{
    public sealed class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
    {
        public CreateProductCommandValidator()
        {
            RuleFor(candidate => candidate.Sku).NotEmpty().MaximumLength(Sku.MaxLength);
            RuleFor(candidate => candidate.Name).NotEmpty().MaximumLength(Product.MaxNameLength);
            RuleFor(candidate => candidate.Description).MaximumLength(Product.MaxDescriptionLength);
            RuleFor(candidate => candidate.Price).GreaterThanOrEqualTo(0);
            RuleFor(candidate => candidate.CategoryId).NotEmpty();
        }
    }
}
