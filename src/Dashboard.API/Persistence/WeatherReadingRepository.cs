using Dapper;
using Dashboard.API.Configuration;
using Dashboard.API.Models;
using Microsoft.Extensions.Options;
using Npgsql;

namespace Dashboard.API.Persistence;

public class WeatherReadingRepository
{
    private const string SelectRecentSql = """
        SELECT id AS Id, station_id AS StationId, temperature_f AS Temperature, humidity_pct AS Humidity,
               pressure_hpa AS Pressure, dew_point_f AS DewPoint, heat_index_f AS HeatIndex,
               absolute_humidity_g_m3 AS AbsoluteHumidity, vapor_pressure_deficit_kpa AS VaporPressureDeficit,
               reading_timestamp AS Timestamp, processed_at AS ProcessedAt
        FROM readings
        WHERE @StationId IS NULL OR station_id = @StationId
        ORDER BY reading_timestamp DESC
        LIMIT @Limit;
        """;

    private const string SelectLatestSql = """
        SELECT id AS Id, station_id AS StationId, temperature_f AS Temperature, humidity_pct AS Humidity,
               pressure_hpa AS Pressure, dew_point_f AS DewPoint, heat_index_f AS HeatIndex,
               absolute_humidity_g_m3 AS AbsoluteHumidity, vapor_pressure_deficit_kpa AS VaporPressureDeficit,
               reading_timestamp AS Timestamp, processed_at AS ProcessedAt
        FROM readings
        WHERE station_id = @StationId
        ORDER BY reading_timestamp DESC
        LIMIT 1;
        """;

    private const string SelectLatestPerStationSql = """
        SELECT DISTINCT ON (station_id)
               id AS Id, station_id AS StationId, temperature_f AS Temperature, humidity_pct AS Humidity,
               pressure_hpa AS Pressure, dew_point_f AS DewPoint, heat_index_f AS HeatIndex,
               absolute_humidity_g_m3 AS AbsoluteHumidity, vapor_pressure_deficit_kpa AS VaporPressureDeficit,
               reading_timestamp AS Timestamp, processed_at AS ProcessedAt
        FROM readings
        ORDER BY station_id, reading_timestamp DESC;
        """;

    private readonly string _connectionString;

    public WeatherReadingRepository(IOptions<PostgresOptions> options)
    {
        _connectionString = options.Value.ConnectionString;
    }

    public async Task<IEnumerable<WeatherReading>> GetRecentAsync(
        string? stationId, int limit, CancellationToken cancellationToken)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        return await connection.QueryAsync<WeatherReading>(
            SelectRecentSql, new { StationId = stationId, Limit = limit });
    }

    public async Task<WeatherReading?> GetLatestAsync(string stationId, CancellationToken cancellationToken)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<WeatherReading>(
            SelectLatestSql, new { StationId = stationId });
    }

    public async Task<IEnumerable<WeatherReading>> GetLatestPerStationAsync(CancellationToken cancellationToken)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        return await connection.QueryAsync<WeatherReading>(SelectLatestPerStationSql);
    }
}
