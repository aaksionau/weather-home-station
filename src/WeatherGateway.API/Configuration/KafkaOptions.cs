namespace WeatherGateway.API.Configuration;

public class KafkaOptions
{
    public const string SectionName = "Kafka";

    public required string BootstrapServers { get; set; }

    public required string WeatherReadingsTopic { get; set; }
}
