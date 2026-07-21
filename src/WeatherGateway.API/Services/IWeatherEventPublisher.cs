using WeatherGateway.API.Models;

namespace WeatherGateway.API.Services;

public interface IWeatherEventPublisher
{
    Task PublishAsync(WeatherReading reading, string correlationId, CancellationToken cancellationToken = default);
}
