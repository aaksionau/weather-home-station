using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Options;
using WeatherRules.Worker.Configuration;
using WeatherRules.Worker.Kafka;
using WeatherRules.Worker.Models;
using WeatherRules.Worker.Persistence;
using WeatherRules.Worker.Rules;

namespace WeatherRules.Worker.Processing;

public class RulesEngineWorker : BackgroundService
{
    private readonly KafkaOptions _kafkaOptions;
    private readonly RulesRepository _rulesRepository;
    private readonly AlertRepository _alertRepository;
    private readonly ILogger<RulesEngineWorker> _logger;
    private readonly IConsumer<string, string> _consumer;
    private readonly IProducer<string, string> _producer;

    public RulesEngineWorker(
        IOptions<KafkaOptions> kafkaOptions,
        RulesRepository rulesRepository,
        AlertRepository alertRepository,
        ILogger<RulesEngineWorker> logger)
    {
        _kafkaOptions = kafkaOptions.Value;
        _rulesRepository = rulesRepository;
        _alertRepository = alertRepository;
        _logger = logger;

        _consumer = KafkaClientFactory.CreateConsumer(_kafkaOptions);
        _producer = KafkaClientFactory.CreateProducer(_kafkaOptions);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await _alertRepository.EnsureSchemaAsync(stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Failed to ensure database schema on startup");
            throw;
        }

        WeatherRuleEvaluator evaluator;
        try
        {
            var workflows = await _rulesRepository.LoadWorkflowsAsync(stoppingToken);
            evaluator = new WeatherRuleEvaluator(workflows);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Failed to load rule definitions from blob storage on startup");
            throw;
        }

        _consumer.Subscribe(_kafkaOptions.ProcessedTopic);

        _logger.LogInformation(
            "Rules engine started: consuming {ProcessedTopic} as group {GroupId}, publishing alerts to {AlertsTopic}",
            _kafkaOptions.ProcessedTopic, _kafkaOptions.ConsumerGroupId, _kafkaOptions.AlertsTopic);

        while (!stoppingToken.IsCancellationRequested)
        {
            ConsumeResult<string, string>? consumeResult = null;
            EnrichedWeatherReading? reading = null;
            try
            {
                consumeResult = _consumer.Consume(stoppingToken);
                if (consumeResult?.Message is null)
                {
                    continue;
                }

                reading = JsonSerializer.Deserialize<EnrichedWeatherReading>(consumeResult.Message.Value)
                    ?? throw new InvalidOperationException("Received an empty weather reading.");

                var alerts = await evaluator.EvaluateAsync(reading, stoppingToken);

                foreach (var alert in alerts)
                {
                    await _alertRepository.InsertAsync(alert, stoppingToken);

                    await _producer.ProduceAsync(_kafkaOptions.AlertsTopic, new Message<string, string>
                    {
                        Key = alert.StationId,
                        Value = JsonSerializer.Serialize(alert)
                    }, stoppingToken);

                    _logger.LogInformation(
                        "Alert triggered for station {StationId}: {RuleName} ({Severity})",
                        alert.StationId, alert.RuleName, alert.Severity);
                }

                _consumer.Commit(consumeResult);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Rules engine stopping");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to evaluate rules for station {StationId} at {TopicPartitionOffset}",
                    reading?.StationId, consumeResult?.TopicPartitionOffset);
            }
        }
    }

    public override void Dispose()
    {
        _consumer.Close();
        _consumer.Dispose();
        _producer.Flush(TimeSpan.FromSeconds(10));
        _producer.Dispose();
        base.Dispose();
    }
}
