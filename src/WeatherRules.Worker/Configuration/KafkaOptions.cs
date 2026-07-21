namespace WeatherRules.Worker.Configuration;

public class KafkaOptions
{
    public const string SectionName = "Kafka";

    public required string BootstrapServers { get; set; }

    public required string ProcessedTopic { get; set; }

    public required string AlertsTopic { get; set; }

    public required string ConsumerGroupId { get; set; }
}
