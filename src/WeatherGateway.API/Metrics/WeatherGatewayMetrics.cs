using System.Diagnostics.Metrics;

namespace WeatherGateway.API.Metrics;

public static class WeatherGatewayMetrics
{
    public const string MeterName = "WeatherGateway.API";

    private static readonly Meter Meter = new(MeterName);

    public static readonly Counter<long> ReadingsPublished = Meter.CreateCounter<long>(
        "weather_readings_published_total",
        description: "Weather readings successfully published to Kafka.");

    public static readonly Counter<long> PublishFailures = Meter.CreateCounter<long>(
        "weather_readings_publish_failures_total",
        description: "Weather readings that failed to publish to Kafka.");
}
