# System context

OrderFlow accepts commerce commands through five independent HTTP APIs. PostgreSQL is the system of record inside each bounded context. Kafka carries versioned integration events between services; no service reads another service's database. External payment and email systems are represented by configurable development adapters.
