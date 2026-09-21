# Outbox and Inbox

`SaveChangesAsync` extracts domain events, maps them to integration contracts, adds serialized outbox rows, and commits both state and messages atomically. Workers select ordered batches with `FOR UPDATE SKIP LOCKED`, publish with idempotent Kafka producers, then mark rows processed.

Consumers check `(event_id, consumer)` inside their local transaction. State changes and the Inbox marker commit together; only then is the Kafka offset committed. This supports at-least-once delivery without duplicate state mutation.
