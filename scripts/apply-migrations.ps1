$ErrorActionPreference = 'Stop'
Push-Location (Join-Path $PSScriptRoot '..')
try {
    foreach ($service in @('catalog', 'inventory', 'ordering', 'payments', 'notifications')) {
        docker compose run --rm --no-deps "$service-api" --migrate
        if ($LASTEXITCODE -ne 0) { throw "Migration failed for $service." }
    }
}
finally { Pop-Location }
