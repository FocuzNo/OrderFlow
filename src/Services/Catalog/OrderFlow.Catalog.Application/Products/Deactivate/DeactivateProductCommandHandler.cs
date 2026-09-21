using FluentValidation;
using MediatR;
using OrderFlow.Catalog.Application.Abstractions.Errors;
using OrderFlow.Catalog.Application.Abstractions.Messaging;
using OrderFlow.Catalog.Application.Abstractions.Persistence;
using OrderFlow.Catalog.Domain.Products;

namespace OrderFlow.Catalog.Application.Products;

public static partial class ProductFeatures
{
    public sealed class DeactivateProductCommandHandler(IProductRepository r)
        : IRequestHandler<DeactivateProductCommand>
    {
        public async Task Handle(DeactivateProductCommand c, CancellationToken ct)
        {
            var p = await Find(r, c.Id, ct);
            p.Deactivate();
            await r.SaveAsync(ct);
        }
    }
}
