using FluentValidation;
using MediatR;
using OrderFlow.Catalog.Application.Abstractions.Messaging;
using OrderFlow.Catalog.Application.Abstractions.Persistence;
using OrderFlow.Catalog.Domain.Products;

namespace OrderFlow.Catalog.Application.Products;

public sealed record ProductDto(Guid Id, string Name, string? Description, decimal Price, DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt)
{
    public static ProductDto From(Product product) => new(product.Id, product.Name, product.Description, product.Price, product.CreatedAt, product.UpdatedAt);
}

public sealed record GetProductQuery(Guid Id) : IQuery<ProductDto>;
public sealed record ListProductsQuery : IQuery<IReadOnlyList<ProductDto>>;
public sealed record UpdateProductCommand(Guid Id, string Name, string? Description, decimal Price) : ICommand<ProductDto>;
public sealed record DeleteProductCommand(Guid Id) : ICommand;

public sealed class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(Product.MaxNameLength);
        RuleFor(x => x.Description).MaximumLength(Product.MaxDescriptionLength);
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
    }
}

public sealed class GetProductQueryHandler(IProductRepository repository) : IRequestHandler<GetProductQuery, ProductDto>
{
    public async Task<ProductDto> Handle(GetProductQuery query, CancellationToken cancellationToken) =>
        ProductDto.From(await repository.GetByIdAsync(query.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Product '{query.Id}' was not found."));
}

public sealed class ListProductsQueryHandler(IProductRepository repository) : IRequestHandler<ListProductsQuery, IReadOnlyList<ProductDto>>
{
    public async Task<IReadOnlyList<ProductDto>> Handle(ListProductsQuery query, CancellationToken cancellationToken) =>
        (await repository.ListAsync(cancellationToken)).Select(ProductDto.From).ToArray();
}

public sealed class UpdateProductCommandHandler(IProductRepository repository) : IRequestHandler<UpdateProductCommand, ProductDto>
{
    public async Task<ProductDto> Handle(UpdateProductCommand command, CancellationToken cancellationToken)
    {
        var product = await repository.GetByIdAsync(command.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Product '{command.Id}' was not found.");
        product.Update(command.Name, command.Description, command.Price);
        await repository.UpdateAsync(product, cancellationToken);
        return ProductDto.From(product);
    }
}

public sealed class DeleteProductCommandHandler(IProductRepository repository) : IRequestHandler<DeleteProductCommand>
{
    public async Task Handle(DeleteProductCommand command, CancellationToken cancellationToken)
    {
        var product = await repository.GetByIdAsync(command.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Product '{command.Id}' was not found.");
        product.MarkDeleted();
        await repository.DeleteAsync(product, cancellationToken);
    }
}
