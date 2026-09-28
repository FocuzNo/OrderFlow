namespace OrderFlow.Inventory.Application.Abstractions.Persistence;

public interface IInboxRepository
{
    Task<bool> ExistsAsync(
        Guid id,
        CancellationToken cancellationToken
    );

    void Add(
        Guid id,
        string type,
        DateTimeOffset processedAt
    );
}
