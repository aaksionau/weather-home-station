using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Options;
using Weather.Contracts;
using WeatherGateway.API.Kafka;
using WeatherGateway.API.Metrics;
using WeatherGateway.API.Models;

namespace WeatherGateway.API.Services;

public class WeatherEventPublisher : IWeatherEventPublisher, IDisposable
{
    private readonly IProducer<string, string> _producer;
    private readonly string _topic;
    private readonly ILogger<WeatherEventPublisher> _logger;

    public WeatherEventPublisher(IOptions<KafkaOptions> options, ILogger<WeatherEventPublisher> logger)
    {
        var kafkaOptions = options.Value;
        _topic = kafkaOptions.WeatherReadingsTopic;
        _logger = logger;

        var config = new ProducerConfig
        {
            BootstrapServers = kafkaOptions.BootstrapServers
        };
        _producer = new ProducerBuilder<string, string>(config).Build();
    }

    public async Task PublishAsync(WeatherReading reading, StationLocation location, string correlationId, CancellationToken cancellationToken = default)
    {
        var readingEvent = new WeatherReadingEvent(
            reading.StationId, location, reading.Temperature, reading.Humidity, reading.Pressure, reading.Timestamp);

        var message = new Message<string, string>
        {
            Key = reading.StationId,
            Value = JsonSerializer.Serialize(readingEvent),
            Headers = new Headers()
        };
        CorrelationIdHeader.Set(message.Headers, correlationId);

        var result = await _producer.ProduceAsync(_topic, message, cancellationToken);

        WeatherGatewayMetrics.ReadingsPublished.Add(1, new KeyValuePair<string, object?>("station_id", reading.StationId));

        _logger.LogInformation(
            "Published weather reading for station {StationId} to {Topic} [{Partition}] at offset {Offset}, correlation {CorrelationId}",
            reading.StationId, result.Topic, result.Partition, result.Offset, correlationId);
    }

    public void Dispose()
    {
        _producer.Flush(TimeSpan.FromSeconds(10));
        _producer.Dispose();
    }
}
