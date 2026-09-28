param(
    [switch]$SkipBuild
)

$ErrorActionPreference = 'Stop'

Push-Location (Join-Path $PSScriptRoot '..')

try {
    docker compose config --quiet
    if ($LASTEXITCODE -ne 0) { throw 'Invalid Docker Compose configuration.' }

    if (-not $SkipBuild) {
        docker compose build
        if ($LASTEXITCODE -ne 0) { throw 'Service image build failed.' }
    }

    docker compose up -d --wait catalog-db inventory-db ordering-db payments-db notifications-db kafka otel-collector
    if ($LASTEXITCODE -ne 0) { throw 'Infrastructure startup failed.' }

    & (Join-Path $PSScriptRoot 'apply-migrations.ps1')

    docker compose up -d --wait catalog-api inventory-api ordering-api payments-api notifications-api
    if ($LASTEXITCODE -ne 0) { throw 'API startup failed. Check docker compose logs.' }

    Write-Host 'APIs are ready. Catalog Scalar: http://localhost:5001/scalar'
    Write-Host 'Other service ports: Inventory 5002, Ordering 5003, Payments 5004, Notifications 5005.'
    Write-Host 'For a fresh Kafka broker, create the required topics before publishing events.'
}
finally {
    Pop-Location
}
