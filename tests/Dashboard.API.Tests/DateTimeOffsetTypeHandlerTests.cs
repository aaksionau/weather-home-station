using Dashboard.API.Helpers;
using Npgsql;

namespace Dashboard.API.Tests;

public class DateTimeOffsetTypeHandlerTests
{
    private readonly DateTimeOffsetTypeHandler _handler = new();

    [Fact]
    public void Parse_DateTimeOffsetValue_ReturnsItUnchanged()
    {
        var value = new DateTimeOffset(2026, 7, 21, 12, 0, 0, TimeSpan.FromHours(2));

        var result = _handler.Parse(value);

        Assert.Equal(value, result);
    }

    [Fact]
    public void Parse_DateTimeValue_IsInterpretedAsUtc()
    {
        var value = new DateTime(2026, 7, 21, 12, 0, 0, DateTimeKind.Unspecified);

        var result = _handler.Parse(value);

        Assert.Equal(TimeSpan.Zero, result.Offset);
        Assert.Equal(new DateTime(2026, 7, 21, 12, 0, 0, DateTimeKind.Utc), result.UtcDateTime);
    }

    [Fact]
    public void Parse_UnsupportedType_ThrowsInvalidCastException()
    {
        Assert.Throws<InvalidCastException>(() => _handler.Parse("not a date"));
    }

    [Fact]
    public void SetValue_AssignsValueOnParameter()
    {
        var parameter = new NpgsqlParameter();
        var value = new DateTimeOffset(2026, 7, 21, 12, 0, 0, TimeSpan.Zero);

        _handler.SetValue(parameter, value);

        Assert.Equal(value, parameter.Value);
    }
}
