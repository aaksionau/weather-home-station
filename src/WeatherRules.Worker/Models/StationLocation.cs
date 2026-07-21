using System.Text.Json.Serialization;

namespace WeatherRules.Worker.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum StationLocation
{
    Inside,
    Outside
}
