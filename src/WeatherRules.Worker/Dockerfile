FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/Weather.Contracts/Weather.Contracts.csproj src/Weather.Contracts/
COPY src/Weather.Shared/Weather.Shared.csproj src/Weather.Shared/
COPY src/WeatherRules.Worker/WeatherRules.Worker.csproj src/WeatherRules.Worker/
RUN dotnet restore src/WeatherRules.Worker/WeatherRules.Worker.csproj

COPY src/Weather.Contracts/ src/Weather.Contracts/
COPY src/Weather.Shared/ src/Weather.Shared/
COPY src/WeatherRules.Worker/ src/WeatherRules.Worker/
RUN dotnet publish src/WeatherRules.Worker/WeatherRules.Worker.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/runtime:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "WeatherRules.Worker.dll"]
