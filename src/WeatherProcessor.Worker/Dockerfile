FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/Weather.Contracts/Weather.Contracts.csproj src/Weather.Contracts/
COPY src/Weather.Shared/Weather.Shared.csproj src/Weather.Shared/
COPY src/WeatherProcessor.Worker/WeatherProcessor.Worker.csproj src/WeatherProcessor.Worker/
RUN dotnet restore src/WeatherProcessor.Worker/WeatherProcessor.Worker.csproj

COPY src/Weather.Contracts/ src/Weather.Contracts/
COPY src/Weather.Shared/ src/Weather.Shared/
COPY src/WeatherProcessor.Worker/ src/WeatherProcessor.Worker/
RUN dotnet publish src/WeatherProcessor.Worker/WeatherProcessor.Worker.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/runtime:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "WeatherProcessor.Worker.dll"]
