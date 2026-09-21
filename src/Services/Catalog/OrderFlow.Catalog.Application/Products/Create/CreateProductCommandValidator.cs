using FluentValidation;
using OrderFlow.Catalog.Domain.Products;

namespace OrderFlow.Catalog.Application.Products.Create;

public sealed class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(command => command.Name)
            .NotEmpty()
            .MaximumLength(Product.MaxNameLength);

        RuleFor(command => command.Description)
            .MaximumLength(Product.MaxDescriptionLength);

        RuleFor(command => command.Price)
            .GreaterThanOrEqualTo(0m);
    }
}
