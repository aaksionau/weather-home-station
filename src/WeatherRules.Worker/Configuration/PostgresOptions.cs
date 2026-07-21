namespace WeatherRules.Worker.Configuration;

public class PostgresOptions
{
    public const string SectionName = "Postgres";

    public required string ConnectionString { get; set; }
}
