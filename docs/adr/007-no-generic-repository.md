# ADR 007: No generic repository

Status: Accepted

Repositories expose only operations needed by their aggregate and use cases. A generic repository would hide query intent, leak persistence semantics, and encourage an anemic domain model, so it is deliberately excluded.
