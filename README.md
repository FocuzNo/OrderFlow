# OrderFlow

OrderFlow — микросервисная backend-платформа для управления каталогом товаров, заказами, складскими остатками, платежами и уведомлениями. Система построена на .NET 10 и PostgreSQL с разделением бизнес-областей на пять независимых сервисов.

Каждый микросервис владеет своей доменной моделью, API, базой данных, DbContext и миграциями. Сервисы не обращаются к чужим базам данных и не используют общий Domain или DbContext.

## Микросервисы

| Сервис | Ответственность | Основной API | Scalar |
| --- | --- | --- | --- |
| Catalog | Товары, категории, цены и статусы | `/api/products` | [localhost:5001/scalar](http://localhost:5001/scalar) |
| Inventory | Склады, остатки и резервирование | `/api/inventory` | [localhost:5002/scalar](http://localhost:5002/scalar) |
| Ordering | Создание, чтение и отмена заказов | `/api/orders` | [localhost:5003/scalar](http://localhost:5003/scalar) |
| Payments | Обработка платежей и возвраты | `/api/payments` | [localhost:5004/scalar](http://localhost:5004/scalar) |
| Notifications | История уведомлений и попытки доставки | `/api/notifications` | [localhost:5005/scalar](http://localhost:5005/scalar) |

Порты относятся к Docker Compose. Полные HTTP-контракты и доступные операции представлены в Scalar каждого сервиса.

## Технологии

- .NET 10, C# 14, ASP.NET Core и FastEndpoints.
- Clean Architecture, Vertical Slice, CQRS и MediatR.
- FluentValidation и DDD: доменные инварианты, value objects и SmartEnum.
- Entity Framework Core 10, Npgsql и PostgreSQL 18.
- Apache Kafka и Confluent.Kafka для развиваемого событийного взаимодействия.
- Serilog, correlation IDs, OpenTelemetry и централизованный ProblemDetails.
- OpenAPI и Scalar.
- Docker Compose, GitHub Actions, xUnit и Testcontainers.

## Архитектура

Каждый сервис разделён на четыре слоя:

| Слой | Ответственность |
| --- | --- |
| Domain | Агрегаты, сущности, value objects и бизнес-правила |
| Application | Commands, queries, handlers, validators и абстракции |
| Infrastructure | EF Core, PostgreSQL, репозитории и внешние адаптеры |
| Api | HTTP endpoints, контракты запросов и обработка ошибок |

Application зависит от Domain; Infrastructure реализует абстракции Application. Api связывает компоненты через dependency injection. EF Core и Kafka не проникают в бизнес-слои.

Путь запроса:

```text
HTTP → FastEndpoints → MediatR → ValidationBehavior
     → Handler → Domain → Repository → UnitOfWork → PostgreSQL
```

Репозитории подготавливают изменения, а обработчики фиксируют их через `IUnitOfWork`. Роль Unit of Work выполняет DbContext соответствующего сервиса.

`OrderFlow.IntegrationEvents` содержит транспортные контракты событий, а не общую бизнес-модель.

### Структура репозитория

```text
src/Services/
├── Catalog/
├── Inventory/
├── Ordering/
├── Payments/
└── Notifications/
    ├── OrderFlow.Notifications.Api/
    ├── OrderFlow.Notifications.Application/
    ├── OrderFlow.Notifications.Domain/
    └── OrderFlow.Notifications.Infrastructure/
OrderFlow.IntegrationEvents/
tests/
scripts/
deploy/
.github/workflows/
```

Четырёхслойная структура повторяется для каждого микросервиса. Функциональность внутри Application и Api сгруппирована по use case.

## Бизнес-операции

- **Catalog:** управление категориями и товарами, изменение цены, данных и статуса товара.
- **Inventory:** управление складами и остатками, резервирование и освобождение товаров. Конкурентные изменения защищены PostgreSQL `xmin`.
- **Ordering:** создание заказа со снимками товаров и адресом доставки, расчёт суммы, чтение и отмена. Начальный статус — `PendingInventory`.
- **Payments:** регистрация результата платежа и возвраты. Уникальность `OrderId` защищает от повторного создания платежа для одного заказа.
- **Notifications:** сохранение истории уведомлений и попыток доставки, отправка и повторная обработка через logging-адаптер.

Платёжный адаптер имитирует результат обработки; реальные списания и отправка email/SMS не выполняются. Создание заказа пока не запускает завершённый распределённый процесс резервирования и оплаты.

## Запуск

Требуются .NET 10 SDK для локальной разработки и Docker с Compose для контейнерного окружения. Команды выполняются из корня репозитория.

### Сборка .NET

```powershell
dotnet tool restore
dotnet restore OrderFlow.slnx
dotnet build OrderFlow.slnx
```

### Docker Compose

Настройки PostgreSQL по умолчанию приведены в `.env.example`. Для переопределения создайте `.env`; локальные пароли не подходят для production.

```powershell
docker compose config --quiet
docker compose build
docker compose up -d --wait catalog-db inventory-db ordering-db payments-db notifications-db
./scripts/apply-migrations.ps1
docker compose up -d
./scripts/create-kafka-topics.ps1
```

Перед созданием топиков дождитесь готовности Kafka. Скрипт создаёт `orderflow.order.created`; автоматическое создание топиков в брокере отключено.

На Linux/macOS миграции применяются через `sh scripts/apply-migrations.sh`. Скрипты запускают API с аргументом `--migrate` и завершают процесс после обновления схемы. При обычном старте API миграции **не применяются автоматически**.

Для запуска одной командой используйте `./scripts/start.ps1`: скрипт собирает образы, запускает инфраструктуру, применяет миграции и ожидает готовности API. Если образы уже собраны, используйте `./scripts/start.ps1 -SkipBuild`. Создание Kafka-топиков остаётся отдельным шагом.

### Запуск из Rider

Для контейнерного окружения выбирайте конфигурацию **Docker Compose** с корневым `docker-compose.yml`, а не отдельный `OrderFlow.Catalog.Api/Dockerfile`. Перед первым запуском выполните `./scripts/start.ps1` в терминале Rider, чтобы подготовить базы и миграции.

Отдельный Dockerfile собирает образ, но не передаёт настройки из Compose и не подключает контейнер к его сети. Ошибка `ConnectionStrings:CatalogDatabase is required` означает, что строка подключения не была передана. Compose задаёт `ConnectionStrings__CatalogDatabase`, подключает API к сети с `catalog-db` и публикует порт 5001. Не запускайте второй экземпляр API на том же порту; для работы с текущим окружением откройте [Catalog Scalar](http://localhost:5001/scalar).

Внутри Compose PostgreSQL доступен по именам `<service>-db`. Порты баз данных на хост не опубликованы. Каждая база хранит данные в отдельном volume. Compose также содержит Kafka и OpenTelemetry Collector.

### Локальный запуск API

Нужен доступный с хоста PostgreSQL с отдельной базой каждого сервиса. Пример для Ordering и PostgreSQL на порту 5432:

```powershell
$env:ConnectionStrings__OrderingDatabase = "Host=localhost;Port=5432;Database=ordering;Username=orderflow;Password=<password>"
$env:Kafka__BootstrapServers = "localhost:9092"
$env:Kafka__OrderCreatedTopic = "orderflow.order.created"
$env:ASPNETCORE_ENVIRONMENT = "Development"

dotnet ef database update --project src/Services/Ordering/OrderFlow.Ordering.Infrastructure --startup-project src/Services/Ordering/OrderFlow.Ordering.Infrastructure
dotnet run --project src/Services/Ordering/OrderFlow.Ordering.Api --no-launch-profile --urls http://localhost:5003
```

Для остальных сервисов используются `ConnectionStrings__CatalogDatabase`, `ConnectionStrings__InventoryDatabase`, `ConnectionStrings__PaymentsDatabase` и `ConnectionStrings__NotificationsDatabase`. Не запускайте локальный API на порту уже работающего контейнера.

## Пример создания заказа

Отправьте `POST http://localhost:5003/api/orders` с `Content-Type: application/json`:

```json
{
  "customerId": "7ec77abe-8ae5-445d-885f-a214ef8fb102",
  "customerEmail": "buyer@example.com",
  "line1": "15 Main Street",
  "city": "Warsaw",
  "postalCode": "00-001",
  "country": "PL",
  "items": [
    {
      "productId": "a3f26e40-7c6a-4d67-b55a-8a6d94b8a101",
      "productName": "Mechanical Keyboard",
      "unitPrice": 129.99,
      "quantity": 2
    }
  ]
}
```

Успешный ответ — `201 Created`, сумма заказа — `259.98`. Полученный `id` используется в `GET /api/orders/{id}`. Email передаётся обычной строкой, без Markdown-ссылки. Данные товаров пока не сверяются с Catalog.

## API и диагностика

| Путь | Назначение |
| --- | --- |
| `/scalar` | Интерактивная документация и выполнение запросов |
| `/openapi/v1.json` | OpenAPI-контракт |
| `/health/live` | Проверка работоспособности процесса |
| `/health/ready` | Проверка соединения с PostgreSQL сервиса |

Списки поддерживают `Page` и `PageSize`: по умолчанию 1 и 20, максимальный размер страницы — 100. Ошибки возвращаются в формате ProblemDetails: `400` для валидации, `404` для отсутствующих ресурсов, `409` для конфликтов и `500` для непредвиденных исключений.

### Ошибка отсутствующей таблицы

PostgreSQL `42P01: relation "orders" does not exist` означает, что требуемой таблицы нет в подключённой базе. Для нового окружения примените миграции:

```powershell
./scripts/apply-migrations.ps1
Invoke-RestMethod "http://localhost:5003/api/orders?page=1&pageSize=10"
```

Health check проверяет соединение, а не схему, поэтому может возвращать `200` до применения миграций. Не удаляйте volumes для устранения этой ошибки. Если база создана старой несовместимой версией схемы, сначала проверьте историю миграций и сделайте резервную копию.

## Тестирование и CI

```powershell
dotnet test OrderFlow.slnx

# Интеграционные тесты с настоящим PostgreSQL
$env:RUN_DOCKER_TESTS = "true"
dotnet test tests/Architecture/OrderFlow.Infrastructure.IntegrationTests
```

Unit tests проверяют доменные правила и обработчики. Integration tests используют изолированные PostgreSQL-контейнеры через Testcontainers: сохранение агрегатов, позиции заказа, уникальность, резервирование и конкурентные изменения. Без `RUN_DOCKER_TESTS=true` контейнерные тесты пропускаются.

GitHub Actions содержит workflows для сборки и тестов .NET, проверки Compose и сборки контейнерных образов.

## Событийное взаимодействие и ограничения

Ordering содержит Kafka producer для `OrderCreatedIntegrationEvent`. Ключ сообщения — `OrderId`, адрес брокера и топик задаются конфигурацией. Сейчас отправка выполняется после сохранения заказа: ошибка Kafka может привести к HTTP 500 при уже сохранённом заказе.

Transactional Outbox, consumers и Saga для сквозного процесса заказа пока не реализованы. Атомарность записи в PostgreSQL и публикации в Kafka не гарантируется. Аутентификация, авторизация и интеграции с реальными платёжными и почтовыми провайдерами остаются отдельными направлениями развития.

Перед production-развёртыванием необходимы настройка секретов, контроль доступа, надёжная доставка событий и проверка лицензий используемых зависимостей.
