using System.Text.Json.Serialization;

namespace WeatherGateway.API.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum StationLocation
{
    // Default value when a message is missing a location (e.g. deserialized
    // from data produced before this field existed) so it fails to match any
    // location-specific rule instead of silently being treated as Inside.
    None,
    Inside,
    Outside
}
