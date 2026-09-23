namespace OrderFlow.Catalog.Application.Products;

public static partial class ProductFeatures
{
    public sealed class ArchiveProductCommandValidator : AbstractValidator<ArchiveProductCommand>
    {
        public ArchiveProductCommandValidator() => RuleFor(candidate => candidate.Id).NotEmpty();
    }
}
