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
        var unitOfWork = new RecordingUnitOfWork();
        var handler = new ProductFeatures.CreateProductCommandHandler(
            products,
            new CategoryRepository(category),
            unitOfWork
        );

        var result = await handler.Handle(
            new("SKU-1", "Notebook", null, 12.5m, category.Id),
            default
        );

        Assert.Equal(result.Id, products.Entity?.Id);
        Assert.Equal("SKU-1", result.Sku);
        Assert.Equal(1, unitOfWork.CommitCount);
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
            .AddSingleton<IUnitOfWork, RecordingUnitOfWork>()
            .AddSingleton<ICategoryRepository>(new CategoryRepository(category))
            .BuildServiceProvider();

        await Assert.ThrowsAsync<ValidationException>(() =>
            services
                .GetRequiredService<ISender>()
                .Send(new ProductFeatures.CreateProductCommand("", "", null, -1, category.Id))
        );
        Assert.Null(products.Entity);
    }

    [Fact]
    public async Task Create_ShouldNotCommit_WhenCategoryIsMissing()
    {
        var products = new ProductRepository();
        var unitOfWork = new RecordingUnitOfWork();
        var handler = new ProductFeatures.CreateProductCommandHandler(
            products,
            new CategoryRepository(Category.Create("Existing", null)),
            unitOfWork
        );

        await Assert.ThrowsAsync<OrderFlow.Catalog.Application.Abstractions.Errors.NotFoundException>(
            () =>
                handler.Handle(new("SKU-2", "Notebook", null, 10m, Guid.NewGuid()), default)
        );

        Assert.Null(products.Entity);
        Assert.Equal(0, unitOfWork.CommitCount);
    }

    [Fact]
    public async Task Create_ShouldPropagateCancellation_ToCommitBoundary()
    {
        var category = Category.Create("Office", null);
        var unitOfWork = new RecordingUnitOfWork();
        var handler = new ProductFeatures.CreateProductCommandHandler(
            new ProductRepository(),
            new CategoryRepository(category),
            unitOfWork
        );
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            handler.Handle(new("SKU-3", "Notebook", null, 10m, category.Id), cancellation.Token)
        );

        Assert.Equal(0, unitOfWork.CommitCount);
    }

    private sealed class RecordingUnitOfWork : IUnitOfWork
    {
        public int CommitCount { get; private set; }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            CommitCount++;
            return Task.FromResult(1);
        }
    }

    private sealed class ProductRepository : IProductRepository
    {
        public void Remove(Product product) => Entity = null;

        public Product? Entity { get; private set; }

        public Task AddAsync(Product product, CancellationToken cancellationToken)
        {
            Entity = product;
            return Task.CompletedTask;
        }

        public Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
            Task.FromResult(Entity?.Id == id ? Entity : null);

        public Task<IReadOnlyList<Product>> ListAsync(
            int page,
            int pageSize,
            string? search,
            string? sort,
            CancellationToken cancellationToken
        ) => Task.FromResult<IReadOnlyList<Product>>(Entity is null ? [] : [Entity]);

        public Task<bool> SkuExistsAsync(
            string sku,
            Guid? excludingId,
            CancellationToken cancellationToken
        ) => Task.FromResult(Entity?.Sku.Value == sku);
    }

    private sealed class CategoryRepository(Category category) : ICategoryRepository
    {
        public void Remove(Category value) { }

        public Task AddAsync(Category value, CancellationToken cancellationToken) =>
            Task.CompletedTask;

        public Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
            Task.FromResult(id == category.Id ? category : null);

        public Task<IReadOnlyList<Category>> ListAsync(CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<Category>>([category]);

        public Task<bool> NameExistsAsync(
            string name,
            Guid? excludingId,
            CancellationToken cancellationToken
        ) => Task.FromResult(false);
    }
}
