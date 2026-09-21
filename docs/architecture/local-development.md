# Local development

Use .NET SDK 10 and Docker Desktop. Copy `.env.example` to `.env`, start the databases, Kafka, and collector, run each API once with `--migrate`, then start all services. Exact commands and ports are in the repository README.

Unit tests require no infrastructure. Set `RUN_DOCKER_TESTS=true` to enable PostgreSQL migration and Kafka round-trip tests. The flag prevents a missing Docker daemon from making the normal inner loop fail.
