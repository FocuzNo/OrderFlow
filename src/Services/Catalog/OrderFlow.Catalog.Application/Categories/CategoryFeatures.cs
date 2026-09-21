using MediatR;
using OrderFlow.Catalog.Application.Abstractions.Errors;
using OrderFlow.Catalog.Application.Abstractions.Messaging;
using OrderFlow.Catalog.Application.Abstractions.Persistence;
using OrderFlow.Catalog.Domain.Categories;
namespace OrderFlow.Catalog.Application.Categories;
public static class CategoryFeatures
{
    public sealed record Dto(Guid Id, string Name, string? Description) { public static Dto From(Category x) => new(x.Id, x.Name, x.Description); }
    public sealed record Create(string Name, string? Description) : ICommand<Dto>;
    public sealed class CreateHandler(ICategoryRepository r) : IRequestHandler<Create, Dto> { public async Task<Dto> Handle(Create c, CancellationToken ct) { if (await r.NameExistsAsync(c.Name, null, ct)) throw new ConflictException("Category name already exists."); var entity = Category.Create(c.Name, c.Description); await r.AddAsync(entity, ct); return Dto.From(entity); } }
    public sealed record GetAll : IQuery<IReadOnlyList<Dto>>;
    public sealed class GetAllHandler(ICategoryRepository r) : IRequestHandler<GetAll, IReadOnlyList<Dto>> { public async Task<IReadOnlyList<Dto>> Handle(GetAll q, CancellationToken ct) => (await r.ListAsync(ct)).Select(Dto.From).ToArray(); }
    public sealed record Update(Guid Id, string Name, string? Description) : ICommand<Dto>;
    public sealed class UpdateHandler(ICategoryRepository r) : IRequestHandler<Update, Dto> { public async Task<Dto> Handle(Update c, CancellationToken ct) { var x = await r.GetByIdAsync(c.Id, ct) ?? throw new NotFoundException("Category was not found."); if (await r.NameExistsAsync(c.Name, c.Id, ct)) throw new ConflictException("Category name already exists."); x.Update(c.Name, c.Description); await r.SaveAsync(ct); return Dto.From(x); } }
}
