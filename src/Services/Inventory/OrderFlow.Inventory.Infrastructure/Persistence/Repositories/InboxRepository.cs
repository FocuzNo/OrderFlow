using OrderFlow.Inventory.Application.Abstractions.Persistence;
using OrderFlow.Inventory.Infrastructure.Persistence.Inbox;

namespace OrderFlow.Inventory.Infrastructure.Persistence.Repositories;

public sealed class InboxRepository(
    InventoryDbContext databaseContext
) : IInboxRepository
{
    public Task<bool> ExistsAsync(
        Guid id,
        CancellationToken cancellationToken
    ) =>
        databaseContext
            .InboxMessages
            .AnyAsync(
                message => message.Id == id,
                cancellationToken
            );

    public void Add(
        Guid id,
        string type,
        DateTimeOffset processedAt
    )
    {
        var message =
            InboxMessage.Create(
                id,
                type,
                processedAt
            );

        databaseContext.InboxMessages.Add(
            message
        );
    }
}
