using System.Diagnostics.Metrics;

namespace WeatherRules.Worker.Metrics;

public static class WeatherRulesMetrics
{
    public const string MeterName = "WeatherRules.Worker";

    private static readonly Meter Meter = new(MeterName);

    public static readonly Counter<long> ReadingsEvaluated = Meter.CreateCounter<long>(
        "weather_readings_evaluated_total",
        description: "Enriched readings evaluated against the rule set.");

    public static readonly Counter<long> AlertsTriggered = Meter.CreateCounter<long>(
        "weather_alerts_triggered_total",
        description: "Alerts triggered by rule evaluation.");

    public static readonly Counter<long> EvaluationFailures = Meter.CreateCounter<long>(
        "weather_rule_evaluation_failures_total",
        description: "Readings that failed rule evaluation.");
}
