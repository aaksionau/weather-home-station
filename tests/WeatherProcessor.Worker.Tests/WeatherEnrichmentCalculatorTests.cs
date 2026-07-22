using Weather.Contracts.Enums;
using Weather.Contracts.Types;
using WeatherProcessor.Worker.Enrichment;

namespace WeatherProcessor.Worker.Tests;

public class WeatherEnrichmentCalculatorTests
{
    private const double Tolerance = 0.01;

    private readonly WeatherEnrichmentCalculator _calculator = new();

    private static WeatherReadingEvent Reading(double temperature, double humidity, double pressure = 1013.0) =>
        new("outside_01", StationLocation.Outside, temperature, humidity, pressure, DateTimeOffset.UtcNow);

    [Fact]
    public void Enrich_PassesThroughIdentityAndRawFields()
    {
        var timestamp = DateTimeOffset.Parse("2026-07-21T12:00:00Z");
        var reading = new WeatherReadingEvent("outside_01", StationLocation.Outside, 95.0, 50.0, 1013.0, timestamp);

        var result = _calculator.Enrich(reading);

        Assert.Equal("outside_01", result.StationId);
        Assert.Equal(StationLocation.Outside, result.Location);
        Assert.Equal(95.0, result.Temperature);
        Assert.Equal(50.0, result.Humidity);
        Assert.Equal(1013.0, result.Pressure);
        Assert.Equal(timestamp, result.Timestamp);
        Assert.True((DateTimeOffset.UtcNow - result.ProcessedAt).Duration() < TimeSpan.FromSeconds(5));
    }

    [Theory]
    [InlineData(95.0, 50.0, 73.39292078228887, 105.21577210000007, 19.803624806096117, 2.811340619248061)]
    [InlineData(70.0, 30.0, 37.13307838804484, 68.11, 5.530365897262225, 1.7528208337034015)]
    [InlineData(85.0, 90.0, 81.72543520081582, 101.78080360000024, 26.50640066343794, 0.4109716762564446)]
    public void Enrich_ComputesDerivedMetrics(
        double temperature, double humidity, double expectedDewPoint, double expectedHeatIndex,
        double expectedAbsoluteHumidity, double expectedVaporPressureDeficit)
    {
        var result = _calculator.Enrich(Reading(temperature, humidity));

        Assert.Equal(expectedDewPoint, result.DewPoint, Tolerance);
        Assert.Equal(expectedHeatIndex, result.HeatIndex, Tolerance);
        Assert.Equal(expectedAbsoluteHumidity, result.AbsoluteHumidity, Tolerance);
        Assert.Equal(expectedVaporPressureDeficit, result.VaporPressureDeficit, Tolerance);
    }

    [Fact]
    public void Enrich_At100PercentHumidity_DewPointEqualsTemperature()
    {
        var result = _calculator.Enrich(Reading(68.0, 100.0));

        Assert.Equal(68.0, result.DewPoint, Tolerance);
        Assert.Equal(0.0, result.VaporPressureDeficit, Tolerance);
    }

    [Fact]
    public void Enrich_BelowHeatIndexRegressionThreshold_UsesSimpleApproximation()
    {
        // Simple formula applies below ~80F average of (simple, actual) temperature.
        var result = _calculator.Enrich(Reading(70.0, 30.0));

        Assert.Equal(68.11, result.HeatIndex, Tolerance);
    }

    [Fact]
    public void Enrich_AtOrAboveHeatIndexRegressionThreshold_UsesRothfuszRegression()
    {
        var result = _calculator.Enrich(Reading(95.0, 50.0));

        Assert.Equal(105.21577210000007, result.HeatIndex, Tolerance);
    }
}
