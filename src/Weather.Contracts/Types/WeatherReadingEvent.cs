using Weather.Contracts.Enums;

namespace Weather.Contracts.Types;

public record WeatherReadingEvent(
    string StationId,
    StationLocation Location,
    double Temperature,
    double Humidity,
    double Pressure,
    DateTimeOffset Timestamp);
