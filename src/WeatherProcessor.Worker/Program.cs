using WeatherProcessor.Worker.Configuration;
using WeatherProcessor.Worker.Enrichment;
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

var host = builder.Build();
host.Run();
