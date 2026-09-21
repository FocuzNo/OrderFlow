# Architecture

## Boundaries

Catalog owns categories and products. Inventory owns warehouses, stock, and reservations. Ordering owns the order state machine and coordinates the distributed workflow. Payments owns payment attempts and refunds. Notifications owns delivery history. Each service has an independent EF Core `DbContext`, migration history, PostgreSQL database, and Kafka consumer group.

Dependencies point inward: API → Application, Infrastructure → Application/Domain, Application → Domain. No service references another service project. `IntegrationContracts` is the deliberately small exception used only for serialized event schemas.

## Persistence and consistency

Aggregate changes and outbox records are committed by the same `SaveChangesAsync` call. Publishing occurs later, so Kafka downtime cannot lose an accepted domain change. Delivery is at least once; consumers achieve effective-once state changes through Inbox uniqueness and a local database transaction. This does not promise global exactly-once delivery.

Every service has an `InitialDistributedArchitecture` migration. Run migrations explicitly with the API executable's `--migrate` argument. Normal application startup does not mutate schema.

## Event topology

| Topic                               | Publishers    | Consumers                          |
| ----------------------------------- | ------------- | ---------------------------------- |
| `orderflow.catalog.events.v1`       | Catalog       | future read models/integrations    |
| `orderflow.ordering.events.v1`      | Ordering      | Inventory, Payments, Notifications |
| `orderflow.inventory.events.v1`     | Inventory     | Ordering                           |
| `orderflow.payments.events.v1`      | Payments      | Ordering                           |
| `orderflow.notifications.events.v1` | Notifications | future audit/integrations          |
| `orderflow.dead-letter.v1`          | all consumers | operations/manual replay           |

Every envelope carries event id, type, schema version, occurrence time, correlation id, causation id, aggregate id, and JSON payload. Aggregate/order id is used as Kafka key to preserve per-aggregate partition ordering.

## Observability and failure handling

Serilog produces structured logs without request bodies, payment details, or raw recipient content. Correlation ids flow through HTTP and event envelopes. OpenTelemetry instruments ASP.NET Core, HttpClient, Kafka activity sources, and application metrics and exports via OTLP. Liveness reports process health; readiness checks PostgreSQL and Kafka.

Retries use bounded backoff. Exhausted messages go to the dead-letter topic and their Kafka offsets are committed only after the DLT publish succeeds. Replaying a previously processed event is harmless because the Inbox record is checked before state mutation.
