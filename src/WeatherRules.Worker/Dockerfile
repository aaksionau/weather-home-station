FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY WeatherRules.Worker.csproj .
RUN dotnet restore WeatherRules.Worker.csproj

COPY . .
RUN dotnet publish WeatherRules.Worker.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/runtime:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "WeatherRules.Worker.dll"]
