using WeatherGateway.API.Models;

namespace WeatherGateway.API.Services;

public interface IWeatherEventPublisher
{
    Task PublishAsync(WeatherReading reading, StationLocation location, string correlationId, CancellationToken cancellationToken = default);
}
