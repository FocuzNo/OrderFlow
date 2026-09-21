# OrderFlow

OrderFlow is an educational backend project built incrementally with .NET microservices.

The goal is to learn production-like architecture incrementally while keeping service boundaries explicit.

Each service owns its own Domain, Application, Infrastructure, and API layers. Domain has no dependencies; Application depends on Domain; Infrastructure depends on Application; and API composes Application and Infrastructure. Services will remain independent.

**Status:** Catalog Product, Create Product, and the EF Core/PostgreSQL persistence foundation are implemented. Database migrations have not been created yet.

Catalog API requires `ConnectionStrings:CatalogDatabase`. Supply it locally through user secrets or the `ConnectionStrings__CatalogDatabase` environment variable.

Planned services: Catalog, Ordering, Inventory, Payments, and Notifications. Only Catalog is present so far.
