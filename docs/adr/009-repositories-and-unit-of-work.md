# ADR 009: Service-owned repositories and UnitOfWork

Status: Accepted

Each service owns IRepository<TEntity>, aggregate-specific repositories and IUnitOfWork. Generic operations stage changes; handlers call SaveChangesAsync. DbContext implements IUnitOfWork directly and both resolve to the same scoped instance. Queries stay aggregate-specific. Catalog and Inventory support removal for their local administrative cases.

There is no shared persistence implementation, automatic migration or messaging persistence in the pre-Kafka phase.
