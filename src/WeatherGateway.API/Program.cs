using WeatherGateway.API.Kafka;
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
    logger.LogInformation(
        "Received weather reading from station {StationId} at {Timestamp}",
        reading.StationId, reading.Timestamp);

    try
    {
        await publisher.PublishAsync(reading, cancellationToken);
    }
    catch (Exception ex)
    {
        logger.LogError(ex,
            "Failed to publish weather reading from station {StationId} to Kafka",
            reading.StationId);
        return Results.Problem("Failed to publish weather reading.", statusCode: StatusCodes.Status502BadGateway);
    }

    return Results.Accepted();
})
.WithName("PublishWeatherReading");

app.Run();
