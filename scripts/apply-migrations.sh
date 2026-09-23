#!/usr/bin/env sh
set -eu
cd "$(dirname "$0")/.."
for service in catalog inventory ordering payments notifications; do
    docker compose run --rm --no-deps "$service-api" --migrate
done
