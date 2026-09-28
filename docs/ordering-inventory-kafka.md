# Ordering ↔ Inventory: Kafka integration

## Ordering Kafka Summary

- Существующие `CreateOrderCommandHandler`, `IOrderCreatedOutboxWriter` и `OrderCreatedOutboxWriter` сохранены. Handler добавляет Order и Outbox в один OrderingDbContext и вызывает один `SaveChangesAsync`.
- Существующий `OrderCreatedIntegrationEvent` содержит стабильный EventId; повтор Outbox не генерирует новый ID.
- Используется generic `IKafkaPublisher / KafkaPublisher` в Ordering.Infrastructure.
- Producer: Confluent.Kafka, `Acks.All`, `EnableIdempotence=true`, delivery timeout 5000ms.
- Topic: `orderflow.order.created`; key: строковый `OrderId`.
- `Messaging/Outbox/OutboxProcessor` публикует фоновой задачей. Только после успешного `ProduceAsync` вызывается `MarkAsProcessed`; изменение `ProcessedAt` фиксируется в БД.
- При ошибке увеличивается RetryCount, сохраняется Error (максимум 2000 символов), ProcessedAt остаётся null. Следующая попытка — на следующем polling cycle. После MaxRetries автоматические попытки прекращаются; запись не удаляется.
- Запрос необработанных сообщений использует PostgreSQL `FOR UPDATE SKIP LOCKED` внутри транзакции. Несколько workers не берут одну запись одновременно.
- Старые неиспользуемые `IOrderCreatedPublisher / OrderCreatedKafkaPublisher` удалены: прямого пути публикации из Application больше нет.

```text
POST /api/orders
  → FastEndpoints → MediatR → validation → CreateOrderCommandHandler
  → Order + OrderCreatedOutboxWriter.Add
  → один SaveChanges: orders + order_items + outbox_messages
  → HTTP 201
  → OutboxProcessor → KafkaPublisher → orderflow.order.created
  → acknowledgement → ProcessedAt → DB commit
```

## Inventory Kafka Summary

- Consumer: `OrderCreatedKafkaConsumer`; group: `orderflow-inventory`.
- Вход: `orderflow.order.created`.
- Выход: `orderflow.inventory.reserved`, `orderflow.inventory.reservation-failed`.
- DLT: `orderflow.inventory.order-created.dlt`.
- `EnableAutoCommit=false`, `EnableAutoOffsetStore=false`.
- Для каждой попытки создаётся новый DI scope; десериализованный event преобразуется в существующий `ReserveOrderInventoryCommand` и отправляется через ISender.
- FluentValidation проверяет EventId, OrderId, список товаров и положительное количество.
- `ReserveOrderInventoryCommandHandler` проверяет Inbox по EventId, группирует одинаковые ProductId, загружает StockItems с reservations и сначала проверяет **все** позиции.
- При успехе вызывает только `StockItem.Reserve(orderId, quantity)`. Один SaveChanges фиксирует остатки, StockReservation, InboxMessage и InventoryReserved Outbox.
- При отсутствии товара или нехватке доступного количества сохраняет только Inbox и InventoryReservationFailed Outbox. До завершения предварительной проверки резервов нет.
- Технические ошибки не преобразуются в InventoryReservationFailed: они выходят из handler и повторяются consumer-ом с новым DbContext.
- Уникальный первичный ключ Inbox защищает от одновременной записи одного EventId. Конфликты xmin/уникальности откатывают весь SaveChanges; новая попытка перечитывает состояние.
- Generic KafkaPublisher используется собственным Inventory OutboxProcessor и DLT-путём consumer-а, но не Application handler.
- Offset commit выполняется после успешной обработки БД, обнаружения уже обработанного EventId либо подтверждённой публикации DLT.
- При ошибке DLT или offset commit consumer закрывается и переподключается с последнего подтверждённого offset: более поздний commit не может пропустить провалившееся сообщение.
- После исчерпания processing retry в DLT отправляются исходные key/value и headers; добавляются original-topic, original-partition, original-offset, error-type.
- Inventory Outbox публикует оба результата с key=OrderId и той же политикой RetryCount/Error/MaxRetries, что и Ordering.

Таблицы Inventory: `warehouses`, `stock_items`, `stock_reservations`, `inbox_messages`, `outbox_messages`. Базы и DbContext сервисов независимы.

```text
OrderCreated → Consumer → ISender → ReserveOrderInventoryCommandHandler
  → Inbox duplicate? → вернуть без изменений → commit offset
  → проверить все StockItems
      → достаточно: Reserve + StockReservation + Inbox + Reserved Outbox
      → недостаточно: Inbox + ReservationFailed Outbox
  → один SaveChanges → commit offset
  → Inventory OutboxProcessor → KafkaPublisher → result topic → ProcessedAt
```

## Файлы

Пути ниже относительно корня репозитория. Существующие пользовательские изменения сохранены; новые параллельные модели не введены.

### Ordering

Изменены:
- `src/Services/Ordering/OrderFlow.Ordering.Infrastructure/Messaging/Outbox/OutboxProcessor.cs`.
- `src/Services/Ordering/OrderFlow.Ordering.Infrastructure/Persistence/Outbox/OutboxMessage.cs`.

Переиспользованы без дублирования: CreateOrderCommandHandler, OrderCreatedOutboxWriter, KafkaPublisher, IKafkaPublisher, KafkaOptions, OutboxOptions, OrderingDbContext, EF mappings и миграции.

Удалены устаревшие прямой publisher и его интерфейс. Существующий Kafka publisher test переведён на generic KafkaPublisher.

### Inventory

Созданы:
- `Application/Inventory/ReserveOrder/ReserveOrderInventoryCommandValidator.cs`.
- `Infrastructure/Messaging/Kafka/IKafkaPublisher.cs`.
- `Infrastructure/Messaging/Kafka/KafkaPublisher.cs`.
- `Infrastructure/Messaging/Outbox/OutboxProcessor.cs`.

Дополнены:
- ReserveOrderInventoryCommandHandler: устранён повтор using.
- ReserveOrderInventoryItem: record расположен рядом с use case; пустой дублирующий тип устранён.
- KafkaOptions, OrderCreatedKafkaConsumer, OutboxMessage, DependencyInjection.
- `Api/appsettings.Development.json` и `Api/Dockerfile`.

Сохранены существующие StockItem, StockReservation, InventoryFeatures, IInventoryRepository, InboxRepository, InventoryOutboxWriter, Inbox/Outbox entities и EF mappings. Существующие contracts InventoryReservedIntegrationEvent, InventoryReservedItem, InventoryReservationFailedIntegrationEvent используются из `OrderFlow.IntegrationEvents/Inventory`.

### Общие файлы и тесты

- `docker-compose.yml`: явно заданы Inventory topics и consumer group.
- `scripts/create-kafka-topics.ps1`: четыре топика, работа из любой директории, неинтерактивный запуск и проверка exit code.
- `README.md` и этот документ.
- `tests/Inventory/OrderFlow.Inventory.UnitTests/InventoryCommandTests.cs`.
- `tests/Architecture/OrderFlow.Infrastructure.IntegrationTests/Ordering/Messaging/OrderCreatedKafkaPublisherTests.cs`.
- Новые `OrderingInventoryKafkaTests.cs`, `InventoryConsumerRetryTests.cs`, `OutboxReliabilityTests.cs` в integration test project.

## Конфигурация

| Параметр | Значение |
| --- | --- |
| Local Rider Kafka:BootstrapServers | localhost:9092 |
| Docker Kafka:BootstrapServers | kafka:19092 |
| Inventory Kafka:ConsumerGroup | orderflow-inventory |
| Kafka:ProcessingMaxRetries | 3 повтора после первой попытки |
| Kafka:RetryDelayMilliseconds | 1000 |
| Kafka:MaxPollIntervalMilliseconds | 300000 |
| Outbox:BatchSize | 20 |
| Outbox:PollingIntervalMilliseconds | 2000 |
| Outbox:MaxRetries | 10 неудачных публикаций |

Options проверяются при запуске. Для большого времени обработки настройте MaxPollIntervalMilliseconds так, чтобы он покрывал все попытки DB processing и DLT publish. Потеря partition assignment может вызвать redelivery; Inbox обеспечивает идемпотентность.

## Миграции и запуск

Новых миграций поверх имеющихся не требуется: EF проверка pending model changes проходит. Для чистой базы применяются все миграции сервиса, включая InitialCreate и существующие FixGeneratedKeys.

Ordering:
- `20260924162301_AddOrderingOutbox`.
- `20260926132104_AddOutboxDeliveryMetadata`.

Inventory:
- `20260926145459_AddInboxMessages`.
- `20260927193814_AddInventoryOutbox`.

Эти миграции уже существовали в рабочей директории; включите их вместе с designer-файлами в будущий commit.

```powershell
docker compose build ordering-api inventory-api
docker compose up -d --wait ordering-db inventory-db kafka
docker compose run --rm --no-deps ordering-api --migrate
docker compose run --rm --no-deps inventory-api --migrate
./scripts/create-kafka-topics.ps1
docker compose up -d --no-deps --wait ordering-api inventory-api
```

Provisioning использует `--create --if-not-exists --partitions 3 --replication-factor 1` для каждого из четырёх топиков. Уже существующие топики не удаляются и не пересоздаются; параметры можно проверить:

```powershell
docker compose exec -T kafka /opt/kafka/bin/kafka-topics.sh --bootstrap-server kafka:19092 --describe --topic 'orderflow.*'
```

## Manual test

1. В Inventory Scalar создайте склад: POST /api/warehouses с Name и Location.
2. Создайте остаток: POST /api/inventory с ProductId, WarehouseId, Sku и Quantity=10.
3. В Ordering Scalar отправьте POST /api/orders с этим ProductId и Quantity=2. Ожидайте HTTP 201.
4. Проверьте Ordering Outbox: key=OrderId, ProcessedAt заполнен.
5. Проверьте Inventory: Inbox содержит EventId исходного события, stock_reservations содержит OrderId/quantity=2, ReservedQuantity увеличился на 2.
6. Inventory Outbox содержит InventoryReservedIntegrationEvent; после публикации ProcessedAt заполнен.
7. Создайте новый заказ с количеством выше доступного или неизвестным ProductId: ожидается InventoryReservationFailedIntegrationEvent, без частичного резерва.
8. Повторная отправка исходного OrderCreated с тем же EventId не должна менять остатки и создавать новые reservations/outbox.
9. Невалидный JSON должен после retry попасть в DLT.

Просмотр результирующих сообщений, завершение через Ctrl+C:

```powershell
docker compose exec -T kafka /opt/kafka/bin/kafka-console-consumer.sh --bootstrap-server kafka:19092 --topic orderflow.inventory.reserved --from-beginning --property print.key=true
```

Для отказов замените topic на `orderflow.inventory.reservation-failed`, для DLT — на `orderflow.inventory.order-created.dlt`.

## Проверка

```powershell
dotnet tool restore
dotnet restore OrderFlow.slnx
dotnet build OrderFlow.slnx
$env:RUN_DOCKER_TESTS = "true"
dotnet test OrderFlow.slnx
```

Тесты проверяют:
- grouping позиций, duplicate EventId, предварительную проверку без частичного резервирования и валидацию;
- настоящий Kafka publish, обе стороны Outbox, успешный результат, бизнес-отказ и DLT;
- bounded technical retry и проброс ошибки DLT (без перехода к commit);
- атомарный rollback Order/Outbox и Stock/Reservation/Inbox/Outbox при ошибке SQL;
- сохранение retry/error, ограничение MaxRetries и ProcessedAt=null при ошибке.

## Гарантии и границы

- At-least-once, не exactly-once. Kafka acknowledgement и отметка Outbox не являются общей транзакцией; сбой между ними допускает дубликат.
- Producer idempotence не заменяет Inbox. Получатели результирующих событий тоже должны дедуплицировать EventId.
- После MaxRetries сообщение остаётся в Outbox для диагностики и ручного повторного запуска после исправления причины; автоматического удаления или скрытого сброса retry нет.
- DLT требует отдельного процесса анализа/replay. Если DLT недоступен, исходный offset не подтверждается.
- SKIP LOCKED допускает параллельную публикацию; строгий порядок бизнес-событий одного агрегата несколькими Outbox workers не гарантируется. В этой цепочке создаётся одно OrderCreated на заказ.
- Ordering пока не читает InventoryReserved/Failed и не меняет статус по этим результатам. Здесь реализована запрошенная цепочка до результирующих Kafka topics, а не Saga.
- Payments и Notifications не изменялись.
