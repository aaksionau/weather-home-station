using System.Text.Json;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Options;
using RulesEngine.Models;
using WeatherRules.Worker.Configuration;

namespace WeatherRules.Worker.Rules;

public class RulesRepository
{
    private static readonly string DefaultRulesPath =
        Path.Combine(AppContext.BaseDirectory, "Rules", "DefaultRules", "weather-alert-rules.json");

    private readonly BlobStorageOptions _options;
    private readonly ILogger<RulesRepository> _logger;

    public RulesRepository(IOptions<BlobStorageOptions> options, ILogger<RulesRepository> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task<List<Workflow>> LoadWorkflowsAsync(CancellationToken cancellationToken)
    {
        var containerClient = new BlobContainerClient(_options.ConnectionString, _options.ContainerName);
        await containerClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken);

        var blobClient = containerClient.GetBlobClient(_options.RulesBlobName);
        if (!await blobClient.ExistsAsync(cancellationToken))
        {
            _logger.LogInformation(
                "Rules blob {Blob} not found in container {Container}; seeding default rules from {DefaultRulesPath}",
                _options.RulesBlobName, _options.ContainerName, DefaultRulesPath);

            await using var seedStream = File.OpenRead(DefaultRulesPath);
            await blobClient.UploadAsync(seedStream, overwrite: false, cancellationToken);
        }

        var download = await blobClient.DownloadContentAsync(cancellationToken);
        var json = download.Value.Content.ToString();

        return JsonSerializer.Deserialize<List<Workflow>>(json)
            ?? throw new InvalidOperationException($"Rules blob '{_options.RulesBlobName}' deserialized to an empty workflow list.");
    }
}
