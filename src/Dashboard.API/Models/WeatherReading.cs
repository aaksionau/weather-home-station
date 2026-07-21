namespace Dashboard.API.Models;

public record WeatherReading(
    long Id,
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
