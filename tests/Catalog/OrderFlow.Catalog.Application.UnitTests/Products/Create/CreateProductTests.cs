using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using OrderFlow.Catalog.Application;
using OrderFlow.Catalog.Application.Abstractions.Persistence;
using OrderFlow.Catalog.Application.Products;
using OrderFlow.Catalog.Domain.Categories;
using OrderFlow.Catalog.Domain.Products;

namespace OrderFlow.Catalog.Application.UnitTests.Products.Create;

public sealed class CreateProductTests
{
    [Fact]
    public async Task Handler_persists_a_product_for_an_existing_category()
    {
        var category = Category.Create("Office", null);
        var products = new ProductRepository();
        var handler = new ProductFeatures.CreateHandler(products, new CategoryRepository(category));

        var result = await handler.Handle(
            new("SKU-1", "Notebook", null, 12.5m, category.Id),
            default
        );

        Assert.Equal(result.Id, products.Entity?.Id);
        Assert.Equal("SKU-1", result.Sku);
    }

    [Fact]
    public async Task Pipeline_rejects_an_invalid_command_before_the_handler()
    {
        var category = Category.Create("Office", null);
        var products = new ProductRepository();
        using var services = new ServiceCollection()
            .AddLogging()
            .AddApplication()
            .AddSingleton<IProductRepository>(products)
            .AddSingleton<ICategoryRepository>(new CategoryRepository(category))
            .BuildServiceProvider();

        await Assert.ThrowsAsync<ValidationException>(() =>
            services
                .GetRequiredService<ISender>()
                .Send(new ProductFeatures.Create("", "", null, -1, category.Id))
        );
        Assert.Null(products.Entity);
    }

    private sealed class ProductRepository : IProductRepository
    {
        public Product? Entity { get; private set; }

        public Task AddAsync(Product product, CancellationToken ct)
        {
            Entity = product;
            return Task.CompletedTask;
        }

        public Task<Product?> GetByIdAsync(Guid id, CancellationToken ct) =>
            Task.FromResult(Entity?.Id == id ? Entity : null);

        public Task<IReadOnlyList<Product>> ListAsync(
            int page,
            int pageSize,
            string? search,
            string? sort,
            CancellationToken ct
        ) => Task.FromResult<IReadOnlyList<Product>>(Entity is null ? [] : [Entity]);

        public Task<bool> SkuExistsAsync(string sku, Guid? excludingId, CancellationToken ct) =>
            Task.FromResult(Entity?.Sku.Value == sku);

        public Task SaveAsync(CancellationToken ct) => Task.CompletedTask;
    }

    private sealed class CategoryRepository(Category category) : ICategoryRepository
    {
        public Task AddAsync(Category value, CancellationToken ct) => Task.CompletedTask;

        public Task<Category?> GetByIdAsync(Guid id, CancellationToken ct) =>
            Task.FromResult(id == category.Id ? category : null);

        public Task<IReadOnlyList<Category>> ListAsync(CancellationToken ct) =>
            Task.FromResult<IReadOnlyList<Category>>([category]);

        public Task<bool> NameExistsAsync(string name, Guid? excludingId, CancellationToken ct) =>
            Task.FromResult(false);

        public Task SaveAsync(CancellationToken ct) => Task.CompletedTask;
    }
}
