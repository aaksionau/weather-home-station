using Weather.Contracts.Enums;

namespace WeatherGateway.API.Models;

// Station ids are assigned by the physical devices as `<location>_<number>`
// (e.g. `outside_01`, `inside_02`); this is the only place that parses the
// location out of the id string, so the rest of the pipeline can carry a
// typed StationLocation instead of re-deriving it from the id.
public static class StationLocationParser
{
    public static bool TryParse(string stationId, out StationLocation location)
    {
        var prefix = stationId.Split('_', 2)[0];

        switch (prefix.ToLowerInvariant())
        {
            case "inside":
                location = StationLocation.Inside;
                return true;
            case "outside":
                location = StationLocation.Outside;
                return true;
            default:
                location = default;
                return false;
        }
    }

    public static StationLocation Parse(string stationId) =>
        TryParse(stationId, out var location)
            ? location
            : throw new ArgumentException(
                $"Station id '{stationId}' does not start with a recognized location ('inside' or 'outside').",
                nameof(stationId));
}
