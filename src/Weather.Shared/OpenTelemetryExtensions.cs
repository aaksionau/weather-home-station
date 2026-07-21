using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;

namespace Weather.Shared;

public static class OpenTelemetryExtensions
{
    // Every service ships identical OTLP logging and the same runtime
    // instrumentation baseline; configureMetrics adds whatever else a given
    // service needs (its own meter, ASP.NET Core instrumentation, custom
    // histogram views) before the OTLP exporter is attached.
    public static IHostApplicationBuilder AddWeatherObservability(
        this IHostApplicationBuilder builder,
        Action<MeterProviderBuilder>? configureMetrics = null)
    {
        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
            logging.AddOtlpExporter();
        });

        builder.Services.AddOpenTelemetry()
            .WithMetrics(metrics =>
            {
                metrics.AddRuntimeInstrumentation();
                configureMetrics?.Invoke(metrics);
                metrics.AddOtlpExporter();
            });

        return builder;
    }
}
