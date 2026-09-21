using Microsoft.EntityFrameworkCore;
using OrderFlow.Catalog.Application.Abstractions.Persistence;
using OrderFlow.Catalog.Domain.Categories;
using OrderFlow.Catalog.Infrastructure.Persistence;
using OrderFlow.Catalog.Infrastructure.Persistence.Repositories;

namespace OrderFlow.Infrastructure.IntegrationTests;

public sealed class RepositoryCommitBoundaryTests
{
    [Fact]
    public async Task Add_ShouldOnlyStageEntity_WithoutConnectingToDatabase()
    {
        var options = new DbContextOptionsBuilder<CatalogDbContext>()
            .UseNpgsql("Host=127.0.0.1;Port=1;Database=unused;Username=unused;Password=unused")
            .Options;
        await using var databaseContext = new CatalogDbContext(options);
        IRepository<Category> repository = new CategoryRepository(databaseContext);
        var category = Category.Create("Office", null);

        await repository.AddAsync(category, default);

        Assert.Equal(EntityState.Added, databaseContext.Entry(category).State);
        Assert.IsAssignableFrom<IUnitOfWork>(databaseContext);
    }
}
