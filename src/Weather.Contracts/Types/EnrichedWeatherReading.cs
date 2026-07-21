using Weather.Contracts.Enums;

namespace Weather.Contracts.Types;

public record EnrichedWeatherReading(
    string StationId,
    StationLocation Location,
    double Temperature,
    double Humidity,
    double Pressure,
    double DewPoint,
    double HeatIndex,
    double AbsoluteHumidity,
    double VaporPressureDeficit,
    DateTimeOffset Timestamp,
    DateTimeOffset ProcessedAt);
