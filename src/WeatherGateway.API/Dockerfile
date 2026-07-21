FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/Weather.Contracts/Weather.Contracts.csproj src/Weather.Contracts/
COPY src/WeatherGateway.API/WeatherGateway.API.csproj src/WeatherGateway.API/
RUN dotnet restore src/WeatherGateway.API/WeatherGateway.API.csproj

COPY src/Weather.Contracts/ src/Weather.Contracts/
COPY src/WeatherGateway.API/ src/WeatherGateway.API/
RUN dotnet publish src/WeatherGateway.API/WeatherGateway.API.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
RUN apt-get update && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "WeatherGateway.API.dll"]
