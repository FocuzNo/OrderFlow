# OrderFlow

OrderFlow is an educational backend project built incrementally with .NET microservices.

The goal is to learn production-like architecture incrementally while keeping service boundaries explicit.

Each service owns its own Domain, Application, Infrastructure, and API layers. Domain has no dependencies; Application depends on Domain; Infrastructure depends on Application; and API composes Application and Infrastructure. Services will remain independent.

**Status:** Catalog architecture foundation with FastEndpoints, MediatR, FluentValidation, and Serilog. Business functionality has not been added.

Planned services: Catalog, Ordering, Inventory, Payments, and Notifications. Only the Catalog skeleton exists so far.
