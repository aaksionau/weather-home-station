using Confluent.Kafka;

namespace Weather.Contracts.Kafka;

// Takes the raw connection settings rather than each service's own KafkaOptions
// type, since this library is referenced by every service and can't depend
// back on any of them.
public static class KafkaClientFactory
{
    public static IConsumer<string, string> CreateConsumer(string bootstrapServers, string consumerGroupId)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = bootstrapServers,
            GroupId = consumerGroupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false
        };

        return new ConsumerBuilder<string, string>(config).Build();
    }

    public static IProducer<string, string> CreateProducer(string bootstrapServers)
    {
        var config = new ProducerConfig
        {
            BootstrapServers = bootstrapServers
        };

        return new ProducerBuilder<string, string>(config).Build();
    }
}
