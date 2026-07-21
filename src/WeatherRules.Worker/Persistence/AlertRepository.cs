using Dapper;
using Microsoft.Extensions.Options;
using Npgsql;
using WeatherRules.Worker.Configuration;
using WeatherRules.Worker.Models;

namespace WeatherRules.Worker.Persistence;

public class AlertRepository
{
    private const string CreateTableSql = """
        CREATE TABLE IF NOT EXISTS alerts (
            id BIGSERIAL PRIMARY KEY,
            station_id TEXT NOT NULL,
            rule_name TEXT NOT NULL,
            severity TEXT NOT NULL,
            message TEXT NOT NULL,
            reading_timestamp TIMESTAMPTZ NOT NULL,
            triggered_at TIMESTAMPTZ NOT NULL
        );
        CREATE INDEX IF NOT EXISTS idx_alerts_station_time
            ON alerts (station_id, triggered_at DESC);
        """;

    private const string InsertSql = """
        INSERT INTO alerts
            (station_id, rule_name, severity, message, reading_timestamp, triggered_at)
        VALUES
            (@StationId, @RuleName, @Severity, @Message, @ReadingTimestamp, @TriggeredAt);
        """;

    private readonly string _connectionString;

    public AlertRepository(IOptions<PostgresOptions> options)
    {
        _connectionString = options.Value.ConnectionString;
    }

    public async Task EnsureSchemaAsync(CancellationToken cancellationToken)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await connection.ExecuteAsync(CreateTableSql);
    }

    public async Task InsertAsync(Alert alert, CancellationToken cancellationToken)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await connection.ExecuteAsync(InsertSql, alert);
    }
}
