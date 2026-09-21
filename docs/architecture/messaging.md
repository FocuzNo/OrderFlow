# Messaging

Kafka topics are bounded-context streams with a `.v1` suffix. Event envelopes include stable ids, schema version, correlation and causation ids. Keys are aggregate identifiers: order id for the order workflow, product id for catalog/inventory, and payment/order identity for payment events. This preserves ordering for changes to the same business stream without forcing unrelated aggregates into one partition.

Consumers disable auto-commit. They commit offsets only after the local database transaction completes. Bounded retries use backoff; exhausted records are copied to `orderflow.dead-letter.v1`.
