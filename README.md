# OrderFlow

OrderFlow is an educational .NET 10 event-driven commerce backend. It contains five autonomous Clean Architecture services, five PostgreSQL databases, Kafka messaging, transactional Outbox/Inbox delivery, OpenTelemetry, and a Docker Compose environment.

## Services and HTTP API

| Service | Port | Main endpoints |
|---|---:|---|
| Catalog | 5001 | categories CRUD; products create/get/list/update, price, activate, deactivate, archive |
| Inventory | 5002 | warehouses; stock create/get/increase/decrease; reservations create/get/confirm/release |
| Ordering | 5003 | orders create/get/by customer; items add/remove; submit/cancel |
| Payments | 5004 | create/process/get/by order; succeed/fail/refund |
| Notifications | 5005 | create/get/by recipient; send/retry |

Every API exposes `/openapi/v1.json`, `/health/live`, and `/health/ready`. Validation and domain failures use centralized RFC 7807 Problem Details. `X-Correlation-ID` is accepted or generated and returned on each request.

## Architecture

Each service owns its `Domain`, `Application`, `Infrastructure`, and `Api` projects and its database. Services do not reference one another. The only shared project is `OrderFlow.IntegrationContracts`, which contains immutable, versioned wire contracts—never domain logic.

The order workflow is asynchronous:

1. Ordering writes an `OrderSubmitted` event to its outbox with the order transaction.
2. Inventory reserves stock idempotently and emits `InventoryReserved` or `InventoryReservationFailed`.
3. Ordering requests payment after inventory succeeds.
4. Payments uses the deterministic development gateway and emits success or failure.
5. Ordering confirms or cancels the order; cancellation releases pending inventory.
6. Notifications handles confirmed orders and records delivery attempts.

Consumers use manual Kafka offset commits and an Inbox unique key `(event_id, consumer)`. Failures retry with bounded backoff and are published to `orderflow.dead-letter.v1` after exhaustion. Outbox workers lock batches with PostgreSQL `FOR UPDATE SKIP LOCKED`.

More detail is in [architecture.md](docs/architecture.md) and the [ADRs](docs/adr).

## Local verification

```powershell
dotnet restore OrderFlow.slnx
dotnet build OrderFlow.slnx --no-restore
dotnet test OrderFlow.slnx --no-build --no-restore
docker compose config
```

Container-backed migration tests are opt-in:

```powershell
$env:RUN_DOCKER_TESTS = "true"
dotnet test tests/Architecture/OrderFlow.Infrastructure.IntegrationTests
```

## Docker Compose

Copy `.env.example` to `.env` and replace the development password if needed. Migrations are explicit and never run automatically in production mode.

```powershell
docker compose up -d catalog-db inventory-db ordering-db payments-db notifications-db kafka otel-collector
docker compose run --rm catalog-api --migrate
docker compose run --rm inventory-api --migrate
docker compose run --rm ordering-api --migrate
docker compose run --rm payments-api --migrate
docker compose run --rm notifications-api --migrate
docker compose up -d
```

OpenAPI documents are then available on ports 5001–5005. Readiness is healthy only when the service database and Kafka are reachable.

## Configuration

Runtime secrets are supplied with environment variables, user secrets, or a secret manager. Important keys are:

- `ConnectionStrings__CatalogDatabase`, `InventoryDatabase`, `OrderingDatabase`, `PaymentsDatabase`, `NotificationsDatabase`
- `Kafka__BootstrapServers`, `Kafka__ConsumerGroup`, `Kafka__MaxRetries`, `Kafka__OutboxBatchSize`
- `DevelopmentPaymentGateway__Succeed`
- `OTEL_EXPORTER_OTLP_ENDPOINT` and `OTEL_EXPORTER_OTLP_PROTOCOL`

The included payment gateway and email sender are intentionally deterministic development adapters. They do not contact a real payment provider or mail server.
