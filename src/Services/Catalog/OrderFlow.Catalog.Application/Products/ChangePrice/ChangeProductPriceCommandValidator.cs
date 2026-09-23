namespace OrderFlow.Catalog.Application.Products;

public static partial class ProductFeatures
{
    public sealed class ChangeProductPriceCommandValidator
        : AbstractValidator<ChangeProductPriceCommand>
    {
        public ChangeProductPriceCommandValidator()
        {
            RuleFor(candidate => candidate.Id).NotEmpty();
            RuleFor(candidate => candidate.Price).GreaterThanOrEqualTo(0);
        }
    }
}
