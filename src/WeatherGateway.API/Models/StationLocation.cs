using System.Text.Json.Serialization;

namespace WeatherGateway.API.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum StationLocation
{
    Inside,
    Outside
}
