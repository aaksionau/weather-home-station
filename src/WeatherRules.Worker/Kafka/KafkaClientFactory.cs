using Confluent.Kafka;
using WeatherRules.Worker.Configuration;

namespace WeatherRules.Worker.Kafka;

public static class KafkaClientFactory
{
    public static IConsumer<string, string> CreateConsumer(KafkaOptions options)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = options.BootstrapServers,
            GroupId = options.ConsumerGroupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false
        };

        return new ConsumerBuilder<string, string>(config).Build();
    }

    public static IProducer<string, string> CreateProducer(KafkaOptions options)
    {
        var config = new ProducerConfig
        {
            BootstrapServers = options.BootstrapServers
        };

        return new ProducerBuilder<string, string>(config).Build();
    }
}
