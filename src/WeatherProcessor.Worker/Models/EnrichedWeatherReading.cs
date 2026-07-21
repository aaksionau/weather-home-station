namespace WeatherProcessor.Worker.Models;

public record EnrichedWeatherReading(
    string StationId,
    double Temperature,
    double Humidity,
    double Pressure,
    double DewPoint,
    double HeatIndex,
    double AbsoluteHumidity,
    double VaporPressureDeficit,
    DateTimeOffset Timestamp,
    DateTimeOffset ProcessedAt);
