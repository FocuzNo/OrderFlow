namespace OrderFlow.Catalog.Application.Products;

public static partial class ProductFeatures
{
    public sealed class ChangeProductPriceCommandValidator
        : AbstractValidator<ChangeProductPriceCommand>
    {
        public ChangeProductPriceCommandValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
        }
    }
}
