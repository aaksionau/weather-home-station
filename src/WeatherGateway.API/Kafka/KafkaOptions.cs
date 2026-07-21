namespace WeatherGateway.API.Kafka;

public class KafkaOptions
{
    public const string SectionName = "Kafka";

    public required string BootstrapServers { get; set; }

    public required string WeatherReadingsTopic { get; set; }
}
