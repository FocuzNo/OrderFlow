# OrderFlow

OrderFlow is an educational backend project built incrementally with .NET microservices.

The goal is to learn production-like architecture incrementally while keeping service boundaries explicit.

Each service owns its own Domain, Application, Infrastructure, and API layers. Domain has no dependencies; Application depends on Domain; Infrastructure depends on Application; and API composes Application and Infrastructure. Services will remain independent.

**Status:** Catalog Product domain model and Create Product use case are implemented. The API starts, but `POST /api/products` cannot persist a product until the next phase provides an `IProductRepository` implementation.

Planned services: Catalog, Ordering, Inventory, Payments, and Notifications. Only Catalog is present so far.
