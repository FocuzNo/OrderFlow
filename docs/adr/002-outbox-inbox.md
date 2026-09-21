# ADR 002: Transactional Outbox and Inbox

Status: Accepted

Domain state and serialized integration events are written in one local PostgreSQL transaction. Background workers publish unprocessed rows with idempotent Kafka producers. Consumers use manual commits and write an Inbox record in the same transaction as their local state change.

The system therefore uses at-least-once transport with idempotent processing. It does not claim distributed exactly-once semantics. Poison messages are retried a bounded number of times and then copied to the dead-letter topic.
