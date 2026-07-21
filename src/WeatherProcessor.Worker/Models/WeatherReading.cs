namespace WeatherProcessor.Worker.Models;

public record WeatherReading(
    string StationId,
    double Temperature,
    double Humidity,
    double Pressure,
    DateTimeOffset Timestamp);
