# system context

The current implementation is the pre-Kafka baseline. See [README](../../README.md) for the supported local and Docker workflows, database ownership, endpoints and test commands. Each service commits its own local state through its own DbContext/IUnitOfWork. There is no distributed order workflow.
