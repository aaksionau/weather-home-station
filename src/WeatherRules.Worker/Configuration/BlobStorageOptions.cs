namespace WeatherRules.Worker.Configuration;

public class BlobStorageOptions
{
    public const string SectionName = "BlobStorage";

    public required string ConnectionString { get; set; }

    public required string ContainerName { get; set; }

    public required string RulesBlobName { get; set; }
}
