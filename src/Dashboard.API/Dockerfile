FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/Weather.Contracts/Weather.Contracts.csproj src/Weather.Contracts/
COPY src/Weather.Shared/Weather.Shared.csproj src/Weather.Shared/
COPY src/Dashboard.API/Dashboard.API.csproj src/Dashboard.API/
RUN dotnet restore src/Dashboard.API/Dashboard.API.csproj

COPY src/Weather.Contracts/ src/Weather.Contracts/
COPY src/Weather.Shared/ src/Weather.Shared/
COPY src/Dashboard.API/ src/Dashboard.API/
RUN dotnet publish src/Dashboard.API/Dashboard.API.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
RUN apt-get update && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Dashboard.API.dll"]
