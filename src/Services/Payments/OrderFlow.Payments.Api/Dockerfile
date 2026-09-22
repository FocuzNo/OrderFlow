FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY Directory.Build.props global.json ./
COPY src/Services/Payments/ src/Services/Payments/
RUN dotnet restore src/Services/Payments/OrderFlow.Payments.Api/OrderFlow.Payments.Api.csproj
RUN dotnet publish src/Services/Payments/OrderFlow.Payments.Api/OrderFlow.Payments.Api.csproj -c Release --no-restore -o /app /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
RUN apt-get update && apt-get install -y --no-install-recommends curl && rm -rf /var/lib/apt/lists/*
WORKDIR /app
COPY --from=build /app .
USER $APP_UID
HEALTHCHECK --interval=10s --timeout=3s --retries=10 CMD curl --fail http://localhost:8080/health/ready || exit 1
ENTRYPOINT ["dotnet", "OrderFlow.Payments.Api.dll"]
