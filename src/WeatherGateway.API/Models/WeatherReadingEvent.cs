namespace WeatherGateway.API.Models;

public record WeatherReadingEvent(
    string StationId,
    StationLocation Location,
    double Temperature,
    double Humidity,
    double Pressure,
    DateTimeOffset Timestamp);
