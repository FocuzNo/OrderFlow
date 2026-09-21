# ADR 006: No global SharedKernel

Status: Accepted

Entity, aggregate, exception, value-object, and domain-event abstractions belong to each bounded context. Small duplication is preferred to compile-time coupling. Only transport contracts are shared.
