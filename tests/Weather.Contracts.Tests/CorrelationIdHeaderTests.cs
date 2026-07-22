using Confluent.Kafka;
using Weather.Contracts.Kafka;

namespace Weather.Contracts.Tests;

public class CorrelationIdHeaderTests
{
    [Fact]
    public void Set_ThenTryGet_RoundTripsTheValue()
    {
        var headers = new Headers();

        CorrelationIdHeader.Set(headers, "abc-123");

        Assert.Equal("abc-123", CorrelationIdHeader.TryGet(headers));
    }

    [Fact]
    public void TryGet_NullHeaders_ReturnsNull()
    {
        Assert.Null(CorrelationIdHeader.TryGet(null));
    }

    [Fact]
    public void TryGet_HeaderMissing_ReturnsNull()
    {
        var headers = new Headers();
        headers.Add("some_other_key", "value"u8.ToArray());

        Assert.Null(CorrelationIdHeader.TryGet(headers));
    }

    [Fact]
    public void TryGet_MultipleValuesForKey_ReturnsLastOne()
    {
        var headers = new Headers();
        CorrelationIdHeader.Set(headers, "first");
        CorrelationIdHeader.Set(headers, "second");

        Assert.Equal("second", CorrelationIdHeader.TryGet(headers));
    }
}
