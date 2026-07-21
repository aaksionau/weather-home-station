using Dapper;
using Dashboard.API.Configuration;
using Dashboard.API.Helpers;
using Dashboard.API.Persistence;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;

SqlMapper.AddTypeHandler(new DateTimeOffsetTypeHandler());

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services
    .AddOptions<PostgresOptions>()
    .Bind(builder.Configuration.GetSection(PostgresOptions.SectionName))
    .ValidateOnStart();

builder.Services.AddSingleton<WeatherReadingRepository>();
builder.Services.AddSingleton<AlertRepository>();

builder.Logging.AddOpenTelemetry(logging =>
{
    logging.IncludeFormattedMessage = true;
    logging.IncludeScopes = true;
    logging.AddOtlpExporter();
});

builder.Services.AddOpenTelemetry()
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddRuntimeInstrumentation()
        .AddView(
            instrumentName: "http.server.request.duration",
            new ExplicitBucketHistogramConfiguration
            {
                Boundaries = [0.005, 0.01, 0.025, 0.05, 0.075, 0.1, 0.25, 0.5, 0.75, 1, 2.5, 5, 7.5, 10]
            })
        .AddOtlpExporter());

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGet("/health", () => Results.Ok("Healthy"))
    .WithName("HealthCheck");

app.MapGet("/api/readings", async (
    string? stationId,
    int? limit,
    WeatherReadingRepository repository,
    CancellationToken cancellationToken) =>
{
    var readings = await repository.GetRecentAsync(stationId, NormalizeLimit(limit), cancellationToken);
    return Results.Ok(readings);
})
.WithName("GetReadings");

app.MapGet("/api/readings/{stationId}/latest", async (
    string stationId,
    WeatherReadingRepository repository,
    CancellationToken cancellationToken) =>
{
    var reading = await repository.GetLatestAsync(stationId, cancellationToken);
    return reading is not null ? Results.Ok(reading) : Results.NotFound();
})
.WithName("GetLatestReading");

app.MapGet("/api/alerts", async (
    string? stationId,
    int? limit,
    AlertRepository repository,
    CancellationToken cancellationToken) =>
{
    var alerts = await repository.GetRecentAsync(stationId, NormalizeLimit(limit), cancellationToken);
    return Results.Ok(alerts);
})
.WithName("GetAlerts");

app.Run();

static int NormalizeLimit(int? limit) => limit is null or <= 0 ? 100 : Math.Min(limit.Value, 500);
