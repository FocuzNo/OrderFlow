namespace OrderFlow.Catalog.Application.Products;

public static partial class ProductFeatures
{
    public sealed class ActivateProductCommandValidator : AbstractValidator<ActivateProductCommand>
    {
        public ActivateProductCommandValidator() => RuleFor(candidate => candidate.Id).NotEmpty();
    }
}
