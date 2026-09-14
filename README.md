# OrderFlow

OrderFlow is an educational backend project built incrementally with .NET microservices.

The goal is to learn production-like architecture by introducing each pattern only when a concrete problem calls for it.

Each service owns its own Domain, Application, Infrastructure, and API layers. Domain has no dependencies; Application depends on Domain; Infrastructure depends on Application; and API composes Application and Infrastructure. Services will remain independent.

**Status:** Phase 1 — repository and Catalog Clean Architecture foundation.

Planned services: Catalog, Ordering, Inventory, Payments, and Notifications. Only the Catalog skeleton exists so far.
