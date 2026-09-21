# Persistence

Each service owns a PostgreSQL connection string, EF Core 10 `DbContext`, snake_case schema, and `InitialDistributedArchitecture` migration. Money uses `numeric(18,2)`. Mutable aggregates use PostgreSQL `xmin` concurrency tokens where concurrent writes are expected. Repository queries use no tracking for read-only lists.

Schema changes are explicit: invoke the relevant API with `--migrate`; ordinary startup never calls `Migrate` or `EnsureCreated`.
