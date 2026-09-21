# ADR 003: Event-driven order workflow

Status: Accepted

Ordering is the workflow authority but does not call other services synchronously. It reacts to Inventory and Payments outcomes and advances its own aggregate state. Compensating cancellation events tell Inventory to release pending reservations.

This choreography keeps service boundaries clear and demonstrates eventual consistency. The tradeoff is that clients must observe order status instead of expecting submit to complete the whole transaction synchronously.
