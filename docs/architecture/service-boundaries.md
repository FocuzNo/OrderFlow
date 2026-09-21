# Service boundaries

- Catalog: product and category lifecycle.
- Inventory: warehouses, stock quantities, and reservations. Product ids are external identifiers.
- Ordering: customer order aggregate and workflow state authority. Product data on items is a snapshot.
- Payments: payment attempts, provider outcome, and refunds. Order ids are external identifiers.
- Notifications: notification lifecycle and delivery attempts.

Only immutable types in `OrderFlow.IntegrationContracts` cross boundaries. Domain and persistence projects are never referenced across services.
