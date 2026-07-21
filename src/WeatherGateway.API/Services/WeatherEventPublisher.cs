using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Options;
using WeatherGateway.API.Kafka;
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

    public async Task PublishAsync(WeatherReading reading, CancellationToken cancellationToken = default)
    {
        var message = new Message<string, string>
        {
            Key = reading.StationId,
            Value = JsonSerializer.Serialize(reading)
        };

        var result = await _producer.ProduceAsync(_topic, message, cancellationToken);

        _logger.LogInformation(
            "Published weather reading for station {StationId} to {Topic} [{Partition}] at offset {Offset}",
            reading.StationId, result.Topic, result.Partition, result.Offset);
    }

    public void Dispose()
    {
        _producer.Flush(TimeSpan.FromSeconds(10));
        _producer.Dispose();
    }
}
