using OrderFlow.Catalog.Application.Abstractions.Errors;
using OrderFlow.Catalog.Application.Abstractions.Messaging;
using OrderFlow.Catalog.Application.Abstractions.Persistence;
using OrderFlow.Catalog.Domain.Categories;

namespace OrderFlow.Catalog.Application.Categories;

public static partial class CategoryFeatures
{
    public sealed record CategoryResponse(Guid Id, string Name, string? Description)
    {
        public static CategoryResponse From(Category candidate) =>
            new(candidate.Id, candidate.Name, candidate.Description);
    }
}
