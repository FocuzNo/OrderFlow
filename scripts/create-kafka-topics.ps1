$ErrorActionPreference = "Stop"

$bootstrapServer = "kafka:19092"

$topics = @(
    @{
        Name = "orderflow.order.created"
        Partitions = 3
        ReplicationFactor = 1
    },
    @{
        Name = "orderflow.inventory.reserved"
        Partitions = 3
        ReplicationFactor = 1
    },
    @{
        Name = "orderflow.inventory.reservation-failed"
        Partitions = 3
        ReplicationFactor = 1
    },
    @{
        Name = "orderflow.inventory.order-created.dlt"
        Partitions = 3
        ReplicationFactor = 1
    }
)

Push-Location (Join-Path $PSScriptRoot '..')
try {
foreach ($topic in $topics) {
    Write-Host "Creating Kafka topic: $($topic.Name)"

    docker compose exec -T kafka `
        /opt/kafka/bin/kafka-topics.sh `
        --bootstrap-server $bootstrapServer `
        --create `
        --if-not-exists `
        --topic $topic.Name `
        --partitions $topic.Partitions `
        --replication-factor $topic.ReplicationFactor
    if ($LASTEXITCODE -ne 0) { throw "Failed to provision topic $($topic.Name)." }
}

}
finally { Pop-Location }

Write-Host "Kafka topics are ready."
