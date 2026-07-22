using Weather.Contracts.Enums;
using WeatherGateway.API.Models;

namespace WeatherGateway.API.Tests;

public class StationLocationParserTests
{
    [Theory]
    [InlineData("outside_01", StationLocation.Outside)]
    [InlineData("inside_02", StationLocation.Inside)]
    [InlineData("OUTSIDE_01", StationLocation.Outside)]
    [InlineData("Inside_Garage", StationLocation.Inside)]
    public void TryParse_RecognizedPrefix_ReturnsTrueAndLocation(string stationId, StationLocation expected)
    {
        var result = StationLocationParser.TryParse(stationId, out var location);

        Assert.True(result);
        Assert.Equal(expected, location);
    }

    [Theory]
    [InlineData("garage_01")]
    [InlineData("01_outside")]
    [InlineData("")]
    public void TryParse_UnrecognizedPrefix_ReturnsFalse(string stationId)
    {
        var result = StationLocationParser.TryParse(stationId, out var location);

        Assert.False(result);
        Assert.Equal(default, location);
    }

    [Fact]
    public void TryParse_NoUnderscore_TreatsWholeIdAsPrefix()
    {
        var result = StationLocationParser.TryParse("inside", out var location);

        Assert.True(result);
        Assert.Equal(StationLocation.Inside, location);
    }

    [Fact]
    public void Parse_RecognizedPrefix_ReturnsLocation()
    {
        var location = StationLocationParser.Parse("outside_03");

        Assert.Equal(StationLocation.Outside, location);
    }

    [Fact]
    public void Parse_UnrecognizedPrefix_ThrowsArgumentException()
    {
        var ex = Assert.Throws<ArgumentException>(() => StationLocationParser.Parse("attic_01"));

        Assert.Contains("attic_01", ex.Message);
        Assert.Equal("stationId", ex.ParamName);
    }
}
