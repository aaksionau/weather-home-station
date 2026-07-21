using System.Text;
using Confluent.Kafka;

namespace WeatherGateway.API.Kafka;

public static class CorrelationIdHeader
{
    public const string Key = "correlation_id";

    public static void Set(Headers headers, string correlationId) =>
        headers.Add(Key, Encoding.UTF8.GetBytes(correlationId));
}
