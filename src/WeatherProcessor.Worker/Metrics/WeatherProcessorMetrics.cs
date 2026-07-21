using System.Diagnostics.Metrics;

namespace WeatherProcessor.Worker.Metrics;

public static class WeatherProcessorMetrics
{
    public const string MeterName = "WeatherProcessor.Worker";

    private static readonly Meter Meter = new(MeterName);

    public static readonly Counter<long> ReadingsProcessed = Meter.CreateCounter<long>(
        "weather_readings_processed_total",
        description: "Weather readings enriched, persisted, and republished.");

    public static readonly Counter<long> ProcessingFailures = Meter.CreateCounter<long>(
        "weather_readings_processing_failures_total",
        description: "Weather readings that failed enrichment or publishing.");

    public static readonly Histogram<double> ProcessingDuration = Meter.CreateHistogram<double>(
        "weather_reading_processing_duration_seconds",
        unit: "s",
        description: "Time to enrich, persist, and republish a single reading.");
}
