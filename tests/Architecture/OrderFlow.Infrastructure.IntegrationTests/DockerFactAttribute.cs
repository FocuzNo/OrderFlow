namespace OrderFlow.Infrastructure.IntegrationTests;

public sealed class DockerFactAttribute : FactAttribute
{
    public DockerFactAttribute()
    {
        if (
            !string.Equals(
                Environment.GetEnvironmentVariable("RUN_DOCKER_TESTS"),
                "true",
                StringComparison.OrdinalIgnoreCase
            )
        )
            Skip =
                "Set RUN_DOCKER_TESTS=true with Docker running to execute PostgreSQL integration tests.";
    }
}
