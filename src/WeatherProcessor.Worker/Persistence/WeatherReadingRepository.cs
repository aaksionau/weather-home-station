using Dapper;
using Microsoft.Extensions.Options;
using Npgsql;
using Weather.Contracts.Configuration;
using Weather.Contracts.Types;

namespace WeatherProcessor.Worker.Persistence;

public class WeatherReadingRepository
{
    private const string CreateTableSql = """
        CREATE TABLE IF NOT EXISTS readings (
            id BIGSERIAL PRIMARY KEY,
            station_id TEXT NOT NULL,
            temperature_f DOUBLE PRECISION NOT NULL,
            humidity_pct DOUBLE PRECISION NOT NULL,
            pressure_hpa DOUBLE PRECISION NOT NULL,
            dew_point_f DOUBLE PRECISION NOT NULL,
            heat_index_f DOUBLE PRECISION NOT NULL,
            absolute_humidity_g_m3 DOUBLE PRECISION NOT NULL,
            vapor_pressure_deficit_kpa DOUBLE PRECISION NOT NULL,
            reading_timestamp TIMESTAMPTZ NOT NULL,
            processed_at TIMESTAMPTZ NOT NULL
        );
        CREATE INDEX IF NOT EXISTS idx_readings_station_time
            ON readings (station_id, reading_timestamp DESC);
        """;

    private const string InsertSql = """
        INSERT INTO readings
            (station_id, temperature_f, humidity_pct, pressure_hpa, dew_point_f, heat_index_f,
             absolute_humidity_g_m3, vapor_pressure_deficit_kpa, reading_timestamp, processed_at)
        VALUES
            (@StationId, @Temperature, @Humidity, @Pressure, @DewPoint, @HeatIndex,
             @AbsoluteHumidity, @VaporPressureDeficit, @Timestamp, @ProcessedAt);
        """;

    private readonly string _connectionString;

    public WeatherReadingRepository(IOptions<PostgresOptions> options)
    {
        _connectionString = options.Value.ConnectionString;
    }

    public async Task EnsureSchemaAsync(CancellationToken cancellationToken)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await connection.ExecuteAsync(CreateTableSql);
    }

    public async Task InsertAsync(EnrichedWeatherReading reading, CancellationToken cancellationToken)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await connection.ExecuteAsync(InsertSql, reading);
    }
}
