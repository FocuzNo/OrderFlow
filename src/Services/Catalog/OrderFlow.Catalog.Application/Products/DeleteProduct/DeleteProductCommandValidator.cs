namespace OrderFlow.Catalog.Application.Products;

public static partial class ProductFeatures
{
    public sealed class DeleteProductCommandValidator : AbstractValidator<DeleteProductCommand>
    {
        public DeleteProductCommandValidator()
        {
            RuleFor(command => command.Id).NotEmpty();
        }
    }
}
