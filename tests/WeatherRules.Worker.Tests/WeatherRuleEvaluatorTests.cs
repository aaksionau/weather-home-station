using System.Text.Json;
using RulesEngine.Models;
using Weather.Contracts.Enums;
using Weather.Contracts.Types;
using WeatherRules.Worker.Services;

namespace WeatherRules.Worker.Tests;

// Exercises WeatherRuleEvaluator against the actual weather-alert-rules.json
// shipped with the worker, so a change to the rules file is caught here
// instead of only being noticed in production.
public class WeatherRuleEvaluatorTests
{
    private static readonly WeatherRuleEvaluator Evaluator = CreateEvaluator();

    private static WeatherRuleEvaluator CreateEvaluator()
    {
        var json = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "weather-alert-rules.json"));
        var workflows = JsonSerializer.Deserialize<List<Workflow>>(json)
            ?? throw new InvalidOperationException("Failed to load test rules file.");
        return new WeatherRuleEvaluator(workflows);
    }

    private static EnrichedWeatherReading Reading(
        StationLocation location,
        double temperature = 70,
        double humidity = 40,
        double pressure = 1015,
        double heatIndex = 70,
        string stationId = "outside_01") =>
        new(
            stationId,
            location,
            temperature,
            humidity,
            pressure,
            DewPoint: 50,
            heatIndex,
            AbsoluteHumidity: 10,
            VaporPressureDeficit: 1,
            Timestamp: DateTimeOffset.Parse("2026-07-21T12:00:00Z"),
            ProcessedAt: DateTimeOffset.UtcNow);

    [Fact]
    public async Task EvaluateAsync_CalmConditions_TriggersNoAlerts()
    {
        var alerts = await Evaluator.EvaluateAsync(Reading(StationLocation.Outside), CancellationToken.None);

        Assert.Empty(alerts);
    }

    [Theory]
    [InlineData(89.9, false)]
    [InlineData(90.0, true)]
    public void OutsideHighTemperatureThreshold_IsInclusiveAtNinety(double temperature, bool shouldTrigger) =>
        AssertTriggers(Reading(StationLocation.Outside, temperature: temperature), "HighTemperatureAlertOutside", shouldTrigger);

    [Theory]
    [InlineData(75.9, false)]
    [InlineData(76.0, true)]
    public void InsideHighTemperatureThreshold_IsInclusiveAtSeventySix(double temperature, bool shouldTrigger) =>
        AssertTriggers(Reading(StationLocation.Inside, temperature: temperature), "HighTemperatureAlertInside", shouldTrigger);

    [Fact]
    public async Task FreezingTemperatureInside_UsesCriticalSeverity()
    {
        var alerts = await Evaluator.EvaluateAsync(
            Reading(StationLocation.Inside, temperature: 40), CancellationToken.None);

        var alert = Assert.Single(alerts);
        Assert.Equal("FreezingTemperatureAlertInside", alert.RuleName);
        Assert.Equal("Critical", alert.Severity);
        Assert.Equal("Indoor temperature is at or below 45F, indicating possible heating failure or burst-pipe risk.", alert.Message);
    }

    [Fact]
    public async Task FreezingTemperatureOutside_UsesWarningSeverity()
    {
        var alerts = await Evaluator.EvaluateAsync(
            Reading(StationLocation.Outside, temperature: 20), CancellationToken.None);

        var alert = Assert.Single(alerts);
        Assert.Equal("FreezingTemperatureAlertOutside", alert.RuleName);
        Assert.Equal("Warning", alert.Severity);
    }

    [Fact]
    public void HighTemperature_DoesNotCrossLocations()
    {
        // Outside-only rule must not fire for an Inside reading, even though the
        // temperature would clear the Outside threshold.
        AssertTriggers(Reading(StationLocation.Inside, temperature: 95), "HighTemperatureAlertOutside", false);
    }

    [Fact]
    public async Task LowPressureAlert_FiresRegardlessOfLocation()
    {
        var alerts = await Evaluator.EvaluateAsync(
            Reading(StationLocation.Inside, pressure: 995), CancellationToken.None);

        Assert.Contains(alerts, a => a.RuleName == "LowPressureAlert");
    }

    [Fact]
    public async Task StormyOutsideConditions_TriggersAllMatchingRulesIndependently()
    {
        var reading = Reading(
            StationLocation.Outside,
            temperature: 95,
            humidity: 95,
            pressure: 995,
            heatIndex: 105,
            stationId: "outside_02");

        var alerts = await Evaluator.EvaluateAsync(reading, CancellationToken.None);

        var ruleNames = alerts.Select(a => a.RuleName).ToList();
        Assert.Contains("HighTemperatureAlertOutside", ruleNames);
        Assert.Contains("HeatIndexDangerAlertOutside", ruleNames);
        Assert.Contains("HighHumidityAlertOutside", ruleNames);
        Assert.Contains("LowPressureAlert", ruleNames);
        Assert.Equal(4, alerts.Count);
        Assert.All(alerts, a => Assert.Equal("outside_02", a.StationId));
        Assert.All(alerts, a => Assert.Equal(reading.Timestamp, a.ReadingTimestamp));
    }

    private static void AssertTriggers(EnrichedWeatherReading reading, string ruleName, bool shouldTrigger)
    {
        var alerts = Evaluator.EvaluateAsync(reading, CancellationToken.None).GetAwaiter().GetResult();

        Assert.Equal(shouldTrigger, alerts.Any(a => a.RuleName == ruleName));
    }
}
