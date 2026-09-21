# OrderFlow

OrderFlow is an educational .NET 10 microservices backend. The pre-Docker/pre-Kafka foundation contains five autonomous services:

- Catalog — `Product`
- Ordering — `Order`
- Inventory — `StockItem`
- Payments — `Payment`
- Notifications — `Notification`

Each service owns four projects (`Domain`, `Application`, `Infrastructure`, `Api`), a PostgreSQL `DbContext`, an `InitialCreate` migration, and its own unit tests. There are no project references between services and no shared domain or database.

## Architecture

API requests enter FastEndpoints and are dispatched with MediatR. FluentValidation runs as a pipeline behavior. Domain aggregates enforce their invariants and raise domain events. EF Core saves the aggregate change and serialized outbox rows atomically in the same `SaveChangesAsync` transaction.

Outbox rows are intentionally not published yet. Kafka producers, consumers, background dispatch, retries, idempotency, Dockerfiles, and Docker Compose belong to the next phase.

## HTTP surface

Each resource exposes:

- `POST /api/{resource}`
- `GET /api/{resource}`
- `GET /api/{resource}/{id}`
- `PUT /api/{resource}/{id}`
- `DELETE /api/{resource}/{id}`

Resources are `products`, `orders`, `stock-items`, `payments`, and `notifications`. Every API also exposes `GET /health` and an OpenAPI document at `/openapi/v1.json`.

Catalog uses product-specific fields. The other educational slices use a compact common shape: `reference`, optional `description`, non-negative `value`, and `status`. Their meanings are service-local (for example order total, stock quantity, payment amount, or notification priority).

## Configuration

Provide one PostgreSQL connection string per API through user secrets or environment variables:

- `ConnectionStrings__CatalogDatabase`
- `ConnectionStrings__OrderingDatabase`
- `ConnectionStrings__InventoryDatabase`
- `ConnectionStrings__PaymentsDatabase`
- `ConnectionStrings__NotificationsDatabase`

## Verification

```powershell
dotnet restore OrderFlow.slnx
dotnet build OrderFlow.slnx --no-restore
dotnet test OrderFlow.slnx --no-build --no-restore
```

The design-time context factories are only for EF tooling and do not configure runtime credentials.
