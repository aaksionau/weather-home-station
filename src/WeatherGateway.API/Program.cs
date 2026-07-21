using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using WeatherGateway.API.Kafka;
using WeatherGateway.API.Metrics;
using WeatherGateway.API.Models;
using WeatherGateway.API.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services
    .AddOptions<KafkaOptions>()
    .Bind(builder.Configuration.GetSection(KafkaOptions.SectionName))
    .ValidateOnStart();
builder.Services.AddSingleton<IWeatherEventPublisher, WeatherEventPublisher>();

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
        .AddMeter(WeatherGatewayMetrics.MeterName)
        .AddView(
            instrumentName: "http.server.request.duration",
            new ExplicitBucketHistogramConfiguration
            {
                Boundaries = [0.005, 0.01, 0.025, 0.05, 0.075, 0.1, 0.25, 0.5, 0.75, 1, 2.5, 5, 7.5, 10]
            })
        .AddOtlpExporter());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/health", () => Results.Ok("Healthy"))
    .WithName("HealthCheck");

app.MapPost("/api/weather-readings", async (
    WeatherReading reading,
    IWeatherEventPublisher publisher,
    ILogger<Program> logger,
    CancellationToken cancellationToken) =>
{
    var correlationId = Guid.NewGuid().ToString();

    logger.LogInformation(
        "Received weather reading from station {StationId} at {Timestamp}, correlation {CorrelationId}",
        reading.StationId, reading.Timestamp, correlationId);

    try
    {
        await publisher.PublishAsync(reading, correlationId, cancellationToken);
    }
    catch (Exception ex)
    {
        WeatherGatewayMetrics.PublishFailures.Add(1, new KeyValuePair<string, object?>("station_id", reading.StationId));
        logger.LogError(ex,
            "Failed to publish weather reading from station {StationId} to Kafka, correlation {CorrelationId}",
            reading.StationId, correlationId);
        return Results.Problem("Failed to publish weather reading.", statusCode: StatusCodes.Status502BadGateway);
    }

    return Results.Accepted();
})
.WithName("PublishWeatherReading");

app.Run();
