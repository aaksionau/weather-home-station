using OpenTelemetry.Metrics;
using Weather.Contracts.Configuration;
using Weather.Shared;
using WeatherProcessor.Worker.Configuration;
using WeatherProcessor.Worker.Enrichment;
using WeatherProcessor.Worker.Metrics;
using WeatherProcessor.Worker.Persistence;
using WeatherProcessor.Worker.Processing;

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddOptions<KafkaOptions>()
    .Bind(builder.Configuration.GetSection(KafkaOptions.SectionName))
    .ValidateOnStart();

builder.Services
    .AddOptions<PostgresOptions>()
    .Bind(builder.Configuration.GetSection(PostgresOptions.SectionName))
    .ValidateOnStart();

builder.Services.AddSingleton<WeatherEnrichmentCalculator>();
builder.Services.AddSingleton<WeatherReadingRepository>();
builder.Services.AddHostedService<WeatherProcessingWorker>();

builder.AddWeatherObservability(metrics => metrics
    .AddMeter(WeatherProcessorMetrics.MeterName)
    .AddView(
        instrumentName: "weather_reading_processing_duration_seconds",
        new ExplicitBucketHistogramConfiguration
        {
            Boundaries = [0.001, 0.0025, 0.005, 0.01, 0.025, 0.05, 0.1, 0.25, 0.5, 1, 2.5, 5]
        }));

var host = builder.Build();
host.Run();
