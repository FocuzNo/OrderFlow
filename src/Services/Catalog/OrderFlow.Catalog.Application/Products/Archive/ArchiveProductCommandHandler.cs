using FluentValidation;
using MediatR;
using OrderFlow.Catalog.Application.Abstractions.Errors;
using OrderFlow.Catalog.Application.Abstractions.Messaging;
using OrderFlow.Catalog.Application.Abstractions.Persistence;
using OrderFlow.Catalog.Domain.Products;

namespace OrderFlow.Catalog.Application.Products;

public static partial class ProductFeatures
{
    public sealed class ArchiveProductCommandHandler(IProductRepository r)
        : IRequestHandler<ArchiveProductCommand>
    {
        public async Task Handle(ArchiveProductCommand c, CancellationToken ct)
        {
            var p = await Find(r, c.Id, ct);
            p.Archive();
            await r.SaveAsync(ct);
        }
    }
}
