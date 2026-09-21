using FluentValidation;
using MediatR;
using OrderFlow.Catalog.Application.Abstractions.Errors;
using OrderFlow.Catalog.Application.Abstractions.Messaging;
using OrderFlow.Catalog.Application.Abstractions.Persistence;
using OrderFlow.Catalog.Domain.Products;
namespace OrderFlow.Catalog.Application.Products;
public static class ProductFeatures
{
    public sealed record Dto(Guid Id, string Sku, string Name, string? Description, decimal Price, Guid CategoryId, string Status, DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt)
    { public static Dto From(Product x) => new(x.Id, x.Sku.Value, x.Name, x.Description, x.Price.Amount, x.CategoryId, x.Status.Name, x.CreatedAt, x.UpdatedAt); }
    public sealed record Create(string Sku, string Name, string? Description, decimal Price, Guid CategoryId) : ICommand<Dto>;
    public sealed class CreateValidator : AbstractValidator<Create> { public CreateValidator() { RuleFor(x => x.Sku).NotEmpty().MaximumLength(Sku.MaxLength); RuleFor(x => x.Name).NotEmpty().MaximumLength(Product.MaxNameLength); RuleFor(x => x.Description).MaximumLength(Product.MaxDescriptionLength); RuleFor(x => x.Price).GreaterThanOrEqualTo(0); RuleFor(x => x.CategoryId).NotEmpty(); } }
    public sealed class CreateHandler(IProductRepository products, ICategoryRepository categories) : IRequestHandler<Create, Dto> { public async Task<Dto> Handle(Create c, CancellationToken ct) { if (await products.SkuExistsAsync(c.Sku, null, ct)) throw new ConflictException("SKU already exists."); if (await categories.GetByIdAsync(c.CategoryId, ct) is null) throw new NotFoundException("Category was not found."); var product = Product.Create(c.Sku, c.Name, c.Description, c.Price, c.CategoryId); await products.AddAsync(product, ct); return Dto.From(product); } }
    public sealed record GetById(Guid Id) : IQuery<Dto>;
    public sealed class GetByIdHandler(IProductRepository repository) : IRequestHandler<GetById, Dto> { public async Task<Dto> Handle(GetById q, CancellationToken ct) => Dto.From(await repository.GetByIdAsync(q.Id, ct) ?? throw new NotFoundException("Product was not found.")); }
    public sealed record GetAll(int Page = 1, int PageSize = 20, string? Search = null, string? Sort = null) : IQuery<IReadOnlyList<Dto>>;
    public sealed class GetAllValidator : AbstractValidator<GetAll> { public GetAllValidator() { RuleFor(x => x.Page).GreaterThan(0); RuleFor(x => x.PageSize).InclusiveBetween(1, 100); } }
    public sealed class GetAllHandler(IProductRepository repository) : IRequestHandler<GetAll, IReadOnlyList<Dto>> { public async Task<IReadOnlyList<Dto>> Handle(GetAll q, CancellationToken ct) => (await repository.ListAsync(q.Page, q.PageSize, q.Search, q.Sort, ct)).Select(Dto.From).ToArray(); }
    public sealed record Update(Guid Id, string Name, string? Description, Guid CategoryId) : ICommand<Dto>;
    public sealed class UpdateHandler(IProductRepository products, ICategoryRepository categories) : IRequestHandler<Update, Dto> { public async Task<Dto> Handle(Update c, CancellationToken ct) { var p = await Find(products, c.Id, ct); if (await categories.GetByIdAsync(c.CategoryId, ct) is null) throw new NotFoundException("Category was not found."); p.Update(c.Name, c.Description, c.CategoryId); await products.SaveAsync(ct); return Dto.From(p); } }
    public sealed record ChangePrice(Guid Id, decimal Price) : ICommand<Dto>;
    public sealed class ChangePriceHandler(IProductRepository repository) : IRequestHandler<ChangePrice, Dto> { public async Task<Dto> Handle(ChangePrice c, CancellationToken ct) { var p = await Find(repository, c.Id, ct); p.ChangePrice(c.Price); await repository.SaveAsync(ct); return Dto.From(p); } }
    public sealed record Activate(Guid Id) : ICommand; public sealed class ActivateHandler(IProductRepository r) : IRequestHandler<Activate> { public async Task Handle(Activate c, CancellationToken ct) { var p = await Find(r, c.Id, ct); p.Activate(); await r.SaveAsync(ct); } }
    public sealed record Deactivate(Guid Id) : ICommand; public sealed class DeactivateHandler(IProductRepository r) : IRequestHandler<Deactivate> { public async Task Handle(Deactivate c, CancellationToken ct) { var p = await Find(r, c.Id, ct); p.Deactivate(); await r.SaveAsync(ct); } }
    public sealed record Archive(Guid Id) : ICommand; public sealed class ArchiveHandler(IProductRepository r) : IRequestHandler<Archive> { public async Task Handle(Archive c, CancellationToken ct) { var p = await Find(r, c.Id, ct); p.Archive(); await r.SaveAsync(ct); } }
    private static async Task<Product> Find(IProductRepository r, Guid id, CancellationToken ct) => await r.GetByIdAsync(id, ct) ?? throw new NotFoundException("Product was not found.");
}
