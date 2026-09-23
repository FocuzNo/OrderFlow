# OrderFlow — pre-Kafka foundation

Educational .NET 10 / C# 14 backend with five independent Clean Architecture bounded contexts. Each service owns Domain, Application, Infrastructure, Api, a PostgreSQL database and one InitialCreate migration. No service references another service project.

Kafka is NOT implemented yet. Transactional Outbox is NOT implemented yet. Distributed order workflow and Saga are NOT implemented yet. These will be learned manually after the local foundation, starting with a naive producer and the dual-write problem.

## Architecture

FastEndpoints request records → MediatR CQRS → FluentValidation pipeline → handler → rich domain model → aggregate-specific repository → IUnitOfWork. Each DbContext implements its service's IUnitOfWork directly. Generic repositories stage changes; handlers commit. EF Core 10/Npgsql and all entity configurations stay in Infrastructure. SmartEnums describe lifecycle states.

Services use Serilog request logging, correlation IDs, centralized ProblemDetails (400 validation, 404 missing, 409 domain/uniqueness/concurrency conflicts, 500 unexpected failure), OpenAPI and PostgreSQL health checks. No real payment provider, SMTP or SMS is contacted.

## Services, ports and APIs

| Service | API port | PostgreSQL host port | Main endpoints |
|---|---:|---:|---|
| Catalog | 5001 | 5433 | GET/POST /api/products; GET/PUT/DELETE /api/products/{id}; categories, product price/status |
| Inventory | 5002 | 5434 | GET/POST /api/inventory; GET/PUT/DELETE /api/inventory/{productId}; warehouses and reservation operations |
| Ordering | 5003 | 5435 | GET/POST /api/orders; GET /api/orders/{id}; cancellation |
| Payments | 5004 | 5436 | GET/POST /api/payments; GET /api/payments/{id}; GET /api/payments/order/{id}; refunds |
| Notifications | 5005 | 5437 | GET/POST /api/notifications; GET /api/notifications/{id}; GET /api/notifications/order/{orderId} |

All services expose GET /health/live, GET /health/ready and /openapi/v1.json. Each API also includes an interactive Scalar UI at /scalar, where you can inspect the contract and execute requests against the running service. Liveness checks the process; readiness checks only the service's own PostgreSQL connection.

Scalar URLs:

- Catalog: http://localhost:5001/scalar
- Inventory: http://localhost:5002/scalar
- Ordering: http://localhost:5003/scalar
- Payments: http://localhost:5004/scalar
- Notifications: http://localhost:5005/scalar

Lists use Page (default 1) and PageSize (default 20, maximum 100). Existing recipient/customer-specific history queries are also retained.

## Workflow A: local .NET with PostgreSQL

Install .NET 10 SDK and PostgreSQL 18 (or start just the Compose PostgreSQL services). Create one database per service. From the repository root:

```powershell
dotnet tool restore
dotnet restore
dotnet build OrderFlow.slnx
$env:ConnectionStrings__CatalogDatabase = "Host=localhost;Port=5433;Database=catalog;Username=orderflow;Password=orderflow_dev_only"
dotnet ef database update --project src/Services/Catalog/OrderFlow.Catalog.Infrastructure --startup-project src/Services/Catalog/OrderFlow.Catalog.Infrastructure
dotnet run --project src/Services/Catalog/OrderFlow.Catalog.Api
```

Repeat with Inventory, Ordering, Payments and Notifications, replacing service name, database and port from the table. EF design-time factories require the corresponding ConnectionStrings__<Service>Database variable. No schema changes run on normal API startup.

## Workflow B: Docker Compose

Development defaults are in .env.example; copy it to .env if customizing credentials. Do not use these credentials outside local development.

```powershell
docker compose config --quiet
docker compose build
docker compose up -d --wait catalog-postgres inventory-postgres ordering-postgres payments-postgres notifications-postgres
./scripts/apply-migrations.ps1
docker compose up -d
Invoke-RestMethod http://localhost:5001/health/live
Invoke-RestMethod http://localhost:5001/health/ready
```

On Linux/macOS use `sh scripts/apply-migrations.sh`. The scripts explicitly run each built API with --migrate and exit; migrations never run during normal API startup. Container DNS uses <service>-postgres, not localhost. Each PostgreSQL instance has its own volume mounted at /var/lib/postgresql, suitable for the PostgreSQL 18 image.

## Local business scenarios

Catalog preserves SKU/category invariants. Create a category before a product. Product updates include Price; DELETE physically removes the local product.

Inventory preserves the existing warehouse/SKU model. Create a warehouse first, then POST /api/inventory with ProductId, WarehouseId, Sku and Quantity. ProductId is globally unique within Inventory. Quantity cannot fall below ReservedQuantity; deletion with reservations is rejected. PostgreSQL xmin protects concurrent stock writes; callers receive 409 and must reload before retrying.

Create an order with CustomerId, CustomerEmail, Line1, City, PostalCode, Country and Items (ProductId, ProductName, UnitPrice, Quantity). Items are required, totals are calculated and the order enters PendingInventory. Product snapshots are trusted locally; Catalog verification is deferred. Internal MarkInventoryReservedCommand → MarkPaymentProcessingCommand → ConfirmOrderCommand enforce transitions. CancelOrderCommand provides cancellation. No remote inventory/payment action is triggered.

POST /api/payments accepts OrderId, Amount, optional Method (Card) and SimulateFailure. It records a simulated succeeded/failed payment atomically; OrderId is unique. No Ordering call occurs. Older explicit processing/status endpoints apply only to a compatible local payment state.

POST /api/notifications accepts OrderId, CustomerId, NotificationType, Recipient, Subject, Body and optional Channel (Email). It records history and logs identifiers. Existing send/retry operations use a logging-only adapter and delivery-attempt history.

## Tests

```powershell
dotnet test OrderFlow.slnx
$env:RUN_DOCKER_TESTS = "true"
dotnet test tests/Architecture/OrderFlow.Infrastructure.IntegrationTests
```

PostgreSQL tests create isolated disposable Testcontainers: product persistence, order items, inventory uniqueness/reservations/concurrency, payment order uniqueness and notification history. No developer database or EF InMemory provider is used. Set RUN_DOCKER_TESTS only with Docker running; otherwise container tests are explicitly skipped.

## Migration reset and limitations

This educational pre-Kafka baseline replaces previous development migrations with InitialCreate. Use fresh databases/volumes. Do not apply this baseline to databases created by the previous distributed version; no destructive reset is automated.

Authentication/authorization, real payment/email integrations, cross-service checks and distributed consistency are intentionally absent. HTTP contracts include existing SKU/category, warehouse and shipping details. Docker runtime verification requires a running Docker daemon.

The selected MediatR version prints a licensing notice: development/testing is permitted by that notice; production use requires reviewing its licensing terms. Existing OpenTelemetry exporters are retained; configure an OTLP collector explicitly if telemetry export is needed (no collector is included in Compose).

See [verification report](docs/pre-kafka-verification.md) for the checks performed and remaining runtime limitations.
