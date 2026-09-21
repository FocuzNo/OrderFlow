# ADR 008: Database per service

Status: Accepted

Catalog, Inventory, Ordering, Payments, and Notifications each own a distinct PostgreSQL database and migration history. Cross-database joins and shared tables are prohibited. Data needed by another service travels through immutable integration events, accepting eventual consistency in exchange for independent ownership and deployment.
