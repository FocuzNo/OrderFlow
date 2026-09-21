namespace OrderFlow.Catalog.Application.Products;

public static partial class ProductFeatures
{
    public sealed class ArchiveProductCommandValidator : AbstractValidator<ArchiveProductCommand>
    {
        public ArchiveProductCommandValidator() => RuleFor(x => x.Id).NotEmpty();
    }
}
