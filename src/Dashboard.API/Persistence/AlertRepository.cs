using Dapper;
using Dashboard.API.Models;
using Microsoft.Extensions.Options;
using Npgsql;
using Weather.Contracts.Configuration;

namespace Dashboard.API.Persistence;

public class AlertRepository
{
    private const string SelectRecentSql = """
        SELECT id AS Id, station_id AS StationId, rule_name AS RuleName, severity AS Severity,
               message AS Message, reading_timestamp AS ReadingTimestamp, triggered_at AS TriggeredAt
        FROM alerts
        WHERE @StationId IS NULL OR station_id = @StationId
        ORDER BY triggered_at DESC
        LIMIT @Limit;
        """;

    private readonly string _connectionString;

    public AlertRepository(IOptions<PostgresOptions> options)
    {
        _connectionString = options.Value.ConnectionString;
    }

    public async Task<IEnumerable<Alert>> GetRecentAsync(
        string? stationId, int limit, CancellationToken cancellationToken)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        return await connection.QueryAsync<Alert>(
            SelectRecentSql, new { StationId = stationId, Limit = limit });
    }
}
