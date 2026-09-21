# ADR 001: Autonomous service boundaries

Status: Accepted

Each service owns its domain model, application use cases, persistence, migrations, and PostgreSQL database. Direct service project references and shared domain abstractions are prohibited. A small immutable integration-contract assembly is allowed because wire compatibility must be explicit and reviewable.

This adds some deliberate duplication, but prevents accidental distributed-monolith coupling and lets every service evolve and deploy independently.
