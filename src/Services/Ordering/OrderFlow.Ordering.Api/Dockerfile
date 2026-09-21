FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore src/Services/Ordering/OrderFlow.Ordering.Api/OrderFlow.Ordering.Api.csproj
RUN dotnet publish src/Services/Ordering/OrderFlow.Ordering.Api/OrderFlow.Ordering.Api.csproj -c Release --no-restore -o /app /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app .
USER $APP_UID
ENTRYPOINT ["dotnet", "OrderFlow.Ordering.Api.dll"]
