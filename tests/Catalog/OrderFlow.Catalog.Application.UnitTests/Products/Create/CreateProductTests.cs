using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using OrderFlow.Catalog.Application;
using OrderFlow.Catalog.Application.Abstractions.Persistence;
using OrderFlow.Catalog.Application.Products.Create;
using OrderFlow.Catalog.Domain.Products;

namespace OrderFlow.Catalog.Application.UnitTests.Products.Create;

public sealed class CreateProductTests
{
    [Fact]
    public async Task Handler_creates_product_persists_it_and_returns_its_values()
    {
        var repository = new RecordingProductRepository();
        var handler = new CreateProductCommandHandler(repository);
        using var cancellation = new CancellationTokenSource();

        var response = await handler.Handle(
            new CreateProductCommand("Notebook", "Ruled", 12.50m),
            cancellation.Token);

        var product = Assert.IsType<Product>(repository.AddedProduct);
        Assert.Equal(cancellation.Token, repository.ReceivedCancellationToken);
        Assert.Equal(product.Id, response.Id);
        Assert.Equal(product.Name, response.Name);
        Assert.Equal(product.Description, response.Description);
        Assert.Equal(product.Price, response.Price);
        Assert.Equal(product.CreatedAt, response.CreatedAt);
        Assert.Equal(product.UpdatedAt, response.UpdatedAt);
    }

    [Fact]
    public async Task MediatR_pipeline_dispatches_valid_command_to_handler()
    {
        var repository = new RecordingProductRepository();
        using var services = CreateServices(repository);
        var sender = services.GetRequiredService<ISender>();

        var response = await sender.Send(new CreateProductCommand("Notebook", null, 0m));

        Assert.NotEqual(Guid.Empty, response.Id);
        Assert.Equal(response.Id, Assert.IsType<Product>(repository.AddedProduct).Id);
    }

    [Fact]
    public async Task MediatR_pipeline_rejects_empty_name_before_handler()
    {
        var repository = new RecordingProductRepository();
        using var services = CreateServices(repository);
        var sender = services.GetRequiredService<ISender>();

        await Assert.ThrowsAsync<ValidationException>(
            () => sender.Send(new CreateProductCommand(" ", null, 10m)));

        Assert.Null(repository.AddedProduct);
    }

    [Fact]
    public async Task MediatR_pipeline_rejects_negative_price_before_handler()
    {
        var repository = new RecordingProductRepository();
        using var services = CreateServices(repository);
        var sender = services.GetRequiredService<ISender>();

        await Assert.ThrowsAsync<ValidationException>(
            () => sender.Send(new CreateProductCommand("Notebook", null, -1m)));

        Assert.Null(repository.AddedProduct);
    }

    private static ServiceProvider CreateServices(IProductRepository repository) =>
        new ServiceCollection()
            .AddLogging()
            .AddApplication()
            .AddSingleton(repository)
            .BuildServiceProvider();

    private sealed class RecordingProductRepository : IProductRepository
    {
        public Product? AddedProduct { get; private set; }
        public CancellationToken ReceivedCancellationToken { get; private set; }

        public Task AddAsync(Product product, CancellationToken cancellationToken)
        {
            AddedProduct = product;
            ReceivedCancellationToken = cancellationToken;

            return Task.CompletedTask;
        }

        public Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
            Task.FromResult(AddedProduct?.Id == id ? AddedProduct : null);

        public Task<IReadOnlyList<Product>> ListAsync(CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<Product>>(AddedProduct is null ? [] : [AddedProduct]);

        public Task UpdateAsync(Product product, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task DeleteAsync(Product product, CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
