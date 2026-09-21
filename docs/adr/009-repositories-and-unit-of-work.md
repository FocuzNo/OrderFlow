# ADR 009: Service-owned repositories and UnitOfWork

Status: Accepted

Each service owns its IRepository<TEntity> and IUnitOfWork abstractions in Application. Generic repositories provide the shared AddAsync operation; aggregate-specific repositories retain their actual lookup and list queries. Update and Remove are not added speculatively: commands mutate tracked aggregates through domain behavior.

The EF repository only stages changes. Command handlers explicitly call IUnitOfWork.SaveChangesAsync. Each service's DbContext implements IUnitOfWork directly and DI resolves both to the same scoped instance. There is no shared persistence library or extra UnitOfWork wrapper.

Domain-event mapping and Outbox insertion remain inside the DbContext save path. Business state and Outbox entries are saved atomically. Consumers keep their explicit outer transaction so command saves, Inbox records and outgoing workflow events commit together before Kafka offsets are acknowledged.

This decision replaces ADR 007 following the revised project conventions. HTTP contracts use records with init properties to preserve parameterless FastEndpoints binding and existing routes and JSON shapes.
