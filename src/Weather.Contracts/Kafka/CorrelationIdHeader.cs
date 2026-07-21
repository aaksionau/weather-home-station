using System.Text;
using Confluent.Kafka;

namespace Weather.Contracts.Kafka;

public static class CorrelationIdHeader
{
    public const string Key = "correlation_id";

    public static void Set(Headers headers, string correlationId) =>
        headers.Add(Key, Encoding.UTF8.GetBytes(correlationId));

    public static string? TryGet(Headers? headers)
    {
        if (headers is null || !headers.TryGetLastBytes(Key, out var bytes))
        {
            return null;
        }

        return Encoding.UTF8.GetString(bytes);
    }
}
