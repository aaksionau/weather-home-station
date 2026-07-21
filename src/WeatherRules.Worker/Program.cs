using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using WeatherRules.Worker.Configuration;
using WeatherRules.Worker.Metrics;
using WeatherRules.Worker.Persistence;
using WeatherRules.Worker.Processing;
using WeatherRules.Worker.Rules;

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddOptions<KafkaOptions>()
    .Bind(builder.Configuration.GetSection(KafkaOptions.SectionName))
    .ValidateOnStart();

builder.Services
    .AddOptions<PostgresOptions>()
    .Bind(builder.Configuration.GetSection(PostgresOptions.SectionName))
    .ValidateOnStart();

builder.Services
    .AddOptions<BlobStorageOptions>()
    .Bind(builder.Configuration.GetSection(BlobStorageOptions.SectionName))
    .ValidateOnStart();

builder.Services.AddSingleton<RulesRepository>();
builder.Services.AddSingleton<AlertRepository>();
builder.Services.AddHostedService<RulesEngineWorker>();

builder.Logging.AddOpenTelemetry(logging =>
{
    logging.IncludeFormattedMessage = true;
    logging.IncludeScopes = true;
    logging.AddOtlpExporter();
});

builder.Services.AddOpenTelemetry()
    .WithMetrics(metrics => metrics
        .AddRuntimeInstrumentation()
        .AddMeter(WeatherRulesMetrics.MeterName)
        .AddOtlpExporter());

var host = builder.Build();
host.Run();
