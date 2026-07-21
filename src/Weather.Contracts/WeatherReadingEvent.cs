namespace Weather.Contracts;

public record WeatherReadingEvent(
    string StationId,
    StationLocation Location,
    double Temperature,
    double Humidity,
    double Pressure,
    DateTimeOffset Timestamp);
