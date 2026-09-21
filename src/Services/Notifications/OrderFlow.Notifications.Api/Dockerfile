FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore src/Services/Notifications/OrderFlow.Notifications.Api/OrderFlow.Notifications.Api.csproj
RUN dotnet publish src/Services/Notifications/OrderFlow.Notifications.Api/OrderFlow.Notifications.Api.csproj -c Release --no-restore -o /app /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app .
USER $APP_UID
ENTRYPOINT ["dotnet", "OrderFlow.Notifications.Api.dll"]
