FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY WeatherProcessor.Worker.csproj .
RUN dotnet restore WeatherProcessor.Worker.csproj

COPY . .
RUN dotnet publish WeatherProcessor.Worker.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/runtime:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "WeatherProcessor.Worker.dll"]
