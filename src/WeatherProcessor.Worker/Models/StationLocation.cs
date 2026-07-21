using System.Text.Json.Serialization;

namespace WeatherProcessor.Worker.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum StationLocation
{
    Inside,
    Outside
}
