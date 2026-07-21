using Weather.Contracts.Types;

namespace WeatherProcessor.Worker.Enrichment;

public class WeatherEnrichmentCalculator
{
    public EnrichedWeatherReading Enrich(WeatherReadingEvent reading)
    {
        var temperatureC = FahrenheitToCelsius(reading.Temperature);

        var dewPointF = CelsiusToFahrenheit(CalculateDewPointC(temperatureC, reading.Humidity));
        var heatIndexF = CalculateHeatIndexF(reading.Temperature, reading.Humidity);
        var absoluteHumidity = CalculateAbsoluteHumidity(temperatureC, reading.Humidity);
        var vaporPressureDeficit = CalculateVaporPressureDeficit(temperatureC, reading.Humidity);

        return new EnrichedWeatherReading(
            reading.StationId,
            reading.Location,
            reading.Temperature,
            reading.Humidity,
            reading.Pressure,
            dewPointF,
            heatIndexF,
            absoluteHumidity,
            vaporPressureDeficit,
            reading.Timestamp,
            DateTimeOffset.UtcNow);
    }

    private static double FahrenheitToCelsius(double f) => (f - 32.0) * 5.0 / 9.0;

    private static double CelsiusToFahrenheit(double c) => c * 9.0 / 5.0 + 32.0;

    // Magnus-Tetens approximation.
    private static double CalculateDewPointC(double temperatureC, double humidityPct)
    {
        const double a = 17.27;
        const double b = 237.7;

        var alpha = (a * temperatureC) / (b + temperatureC) + Math.Log(humidityPct / 100.0);
        return (b * alpha) / (a - alpha);
    }

    // NWS Rothfusz regression; falls back to the simpler approximation below ~80F where the regression is unreliable.
    private static double CalculateHeatIndexF(double temperatureF, double humidityPct)
    {
        var simple = 0.5 * (temperatureF + 61.0 + (temperatureF - 68.0) * 1.2 + humidityPct * 0.094);
        if ((simple + temperatureF) / 2.0 < 80.0)
        {
            return simple;
        }

        var t = temperatureF;
        var r = humidityPct;

        var heatIndex = -42.379
            + 2.04901523 * t
            + 10.14333127 * r
            - 0.22475541 * t * r
            - 0.00683783 * t * t
            - 0.05481717 * r * r
            + 0.00122874 * t * t * r
            + 0.00085282 * t * r * r
            - 0.00000199 * t * t * r * r;

        if (r < 13 && t is >= 80 and <= 112)
        {
            heatIndex -= (13 - r) / 4.0 * Math.Sqrt((17 - Math.Abs(t - 95.0)) / 17.0);
        }
        else if (r > 85 && t is >= 80 and <= 87)
        {
            heatIndex += (r - 85) / 10.0 * ((87 - t) / 5.0);
        }

        return heatIndex;
    }

    // Bolton (1980) saturation vapor pressure, converted from hPa to absolute humidity in g/m^3.
    private static double CalculateAbsoluteHumidity(double temperatureC, double humidityPct)
    {
        var saturationVaporPressureHpa = 6.112 * Math.Exp((17.67 * temperatureC) / (temperatureC + 243.5));
        return saturationVaporPressureHpa * humidityPct * 2.1674 / (273.15 + temperatureC);
    }

    // Tetens equation, result in kPa.
    private static double CalculateVaporPressureDeficit(double temperatureC, double humidityPct)
    {
        var saturationVaporPressureKpa = 0.6108 * Math.Exp((17.27 * temperatureC) / (temperatureC + 237.3));
        var actualVaporPressureKpa = saturationVaporPressureKpa * (humidityPct / 100.0);
        return saturationVaporPressureKpa - actualVaporPressureKpa;
    }
}
