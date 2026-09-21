namespace OrderFlow.Catalog.Application.Products;

public static partial class ProductFeatures
{
    public sealed class GetProductByIdQueryValidator : AbstractValidator<GetProductByIdQuery>
    {
        public GetProductByIdQueryValidator() => RuleFor(query => query.Id).NotEmpty();
    }
}
