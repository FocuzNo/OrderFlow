namespace OrderFlow.Catalog.Application.Products;

public static partial class ProductFeatures
{
    public sealed class DeactivateProductCommandValidator
        : AbstractValidator<DeactivateProductCommand>
    {
        public DeactivateProductCommandValidator() => RuleFor(x => x.Id).NotEmpty();
    }
}
