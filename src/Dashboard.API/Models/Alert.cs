namespace Dashboard.API.Models;

public record Alert(
    long Id,
    string StationId,
    string RuleName,
    string Severity,
    string Message,
    DateTimeOffset ReadingTimestamp,
    DateTimeOffset TriggeredAt);
