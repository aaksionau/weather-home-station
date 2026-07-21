namespace WeatherRules.Worker.Models;

public record Alert(
    string StationId,
    string RuleName,
    string Severity,
    string Message,
    DateTimeOffset ReadingTimestamp,
    DateTimeOffset TriggeredAt);
