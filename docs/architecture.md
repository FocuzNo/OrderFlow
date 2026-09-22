# Architecture

See the [current pre-Kafka README](../README.md) for routes, startup and limitations.

Five service-owned Clean Architecture stacks use FastEndpoints, record contracts, CQRS/MediatR, FluentValidation, domain invariants, EF Core PostgreSQL repositories and explicit UnitOfWork commits. There are no cross-service project references, transport contracts, messaging workers or shared business models. Internal order transitions are local application commands only.
