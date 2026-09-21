# ADR 005: Aggregate partition keys

Status: Accepted

Events use the relevant aggregate identity as message key, primarily order id for workflow events and product id for catalog/inventory streams. This gives deterministic per-aggregate ordering while allowing unrelated aggregates to scale across partitions.
