# ADR 004: Inbox idempotency

Status: Accepted

Every consumer stores a composite `(event_id, consumer)` key in its own database transaction. Duplicate delivery is acknowledged without executing the application command again. Kafka offset commit follows the database commit.
