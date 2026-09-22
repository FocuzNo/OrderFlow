# Pre-Kafka verification — 2026-09-22

## Scope

The latest implementation request supersedes the earlier distributed-system scope. Kafka transport, shared integration contracts, Outbox/Inbox tables, messaging-only domain events and distributed workflow adapters were removed. No Kafka, Outbox or Saga implementation remains. No new commit, push or pull request was created. Existing stash was left unchanged.

There are 20 service projects: Domain, Application, Infrastructure and Api for each of Catalog, Ordering, Inventory, Payments and Notifications. Seven test projects cover domain/application behavior and persistence. Project-reference inspection found no cross-service dependency, no Domain reference to outer layers and no Application reference to Infrastructure/Api.

## Domain and use cases

| Service | Domain and invariants | Main use cases |
|---|---|---|
| Catalog | Product, Category; SKU/Money value objects; required/limited names, nonnegative price, category and SKU identity | Product create/read/list/update/delete, price/status changes; categories |
| Ordering | Order/OrderItem; email/shipping value objects; required snapshots, positive quantities, calculated total and guarded lifecycle | Create/read/list, customer history, cancellation; internal inventory-reserved/payment-processing/confirmation commands |
| Inventory | Existing StockItem, Warehouse, StockReservation model retained; unique ProductId, nonnegative stock, reservations bounded by stock | Inventory create/read/list/update/delete; reservation and warehouse operations |
| Payments | Payment/Refund; positive amount, unique OrderId, guarded state transitions and refund limits | Simulated success/failure, read/list/by-order, local refund/state operations |
| Notifications | Notification/DeliveryAttempt; required OrderId/CustomerId/type, validated recipient/body | Record/read/list/by-order/by-recipient, logging-only delivery/retry |

SmartEnums include ProductStatus, OrderStatus, ReservationStatus, PaymentStatus/PaymentMethod and NotificationStatus/NotificationChannel. EF configurations own their conversions. Existing domain naming was preserved: Inventory uses StockItem/QuantityOnHand; Ordering exposes TotalAmount; Notifications uses Body.

Each write slice has a command, handler and input validator; read slices use queries. MediatR dispatches through the validation pipeline. HTTP contracts are records in separate files, domain mutations remain encapsulated, and repeated imports use GlobalUsings. Source formatting was reapplied, including member spacing.

## Persistence

CatalogDbContext, OrderingDbContext, InventoryDbContext, PaymentsDbContext and NotificationsDbContext each implement their own IUnitOfWork. Service-owned generic repositories stage changes and aggregate-specific repositories implement relevant queries. Handlers commit explicitly.

Each context has its own InitialCreate migration and snapshot. All five design-time contexts were created successfully; migrations list successfully without a database connection; EF reports no pending model changes. This does not prove migration application against PostgreSQL.

Configurations define relationships, required values, lengths, money precision (18,2), SmartEnum conversions and indexes. Important unique indexes: Catalog SKU/category name, Inventory ProductId, Payments OrderId. Ordering customer/date and Notifications order/recipient indexes support read use cases. PostgreSQL xmin provides optimistic concurrency for aggregate writes, including stock reservations.

The old distributed-development migration baseline was replaced. Only fresh databases/volumes should use these InitialCreate migrations. No existing database or volume was deleted or migrated.

## HTTP and operations

Main routes and ports are listed in README. Catalog and Inventory expose full administrative CRUD; Ordering uses create/read plus explicit lifecycle commands; Payments uses simulation/read/refunds; Notifications records and reads history.

Every API has Serilog request logging, centralized ProblemDetails (400/404/409/500), OpenAPI at /openapi/v1.json and health/live plus health/ready. Readiness checks only the owning database.

Five multi-stage Dockerfiles use .NET 10 and non-root runtime users. Compose contains only five APIs and five PostgreSQL 18 instances with separate databases/volumes. Connection strings use service DNS names. Explicit migration scripts invoke --migrate; normal API startup does not modify schemas.

## Verification

- dotnet tool restore and dotnet restore succeeded.
- Solution build: zero errors and zero warnings.
- Tests: 36 passed; 6 PostgreSQL Testcontainers tests explicitly skipped.
- Container tests cover product persistence, order items, inventory reservation/uniqueness/concurrency, payment uniqueness and notification history.
- Docker Compose configuration validation succeeded.
- Docker daemon is unavailable (dockerDesktopLinuxEngine pipe missing). Images, Compose startup, actual migration application and persisted HTTP CRUD were therefore not verified.
- Local API startup/liveness and nonempty OpenAPI were checked for all five services during this session.
- Notifications readiness returned 503 with a deliberately unavailable database.
- Invalid Notifications and Ordering POST requests returned 400; Ordering empty/null/null-element item collections returned application/problem+json.
- Project-reference audit and source search found no cross-service references or Kafka/Outbox/Inbox/Saga implementation.
- Git diff whitespace check passed. HEAD remains cf9fa3f; the existing stash is untouched.

## Deliberate limits

No authentication, real payment gateway, SMTP/SMS, cross-service product verification or distributed consistency. Order snapshots are caller supplied. Legacy draft-only order editing and explicit payment-state endpoints remain state guarded; newly created orders/payments immediately enter their current creation workflow states. A running Docker daemon is needed to complete the real PostgreSQL and container checks documented in README.
