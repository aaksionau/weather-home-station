using System.Diagnostics;
using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Options;
using WeatherProcessor.Worker.Configuration;
using WeatherProcessor.Worker.Enrichment;
using WeatherProcessor.Worker.Kafka;
using WeatherProcessor.Worker.Metrics;
using WeatherProcessor.Worker.Models;
using WeatherProcessor.Worker.Persistence;

namespace WeatherProcessor.Worker.Processing;

public class WeatherProcessingWorker : BackgroundService
{
    private readonly KafkaOptions _kafkaOptions;
    private readonly WeatherEnrichmentCalculator _calculator;
    private readonly WeatherReadingRepository _repository;
    private readonly ILogger<WeatherProcessingWorker> _logger;
    private readonly IConsumer<string, string> _consumer;
    private readonly IProducer<string, string> _producer;

    public WeatherProcessingWorker(
        IOptions<KafkaOptions> kafkaOptions,
        WeatherEnrichmentCalculator calculator,
        WeatherReadingRepository repository,
        ILogger<WeatherProcessingWorker> logger)
    {
        _kafkaOptions = kafkaOptions.Value;
        _calculator = calculator;
        _repository = repository;
        _logger = logger;

        _consumer = KafkaClientFactory.CreateConsumer(_kafkaOptions);
        _producer = KafkaClientFactory.CreateProducer(_kafkaOptions);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await _repository.EnsureSchemaAsync(stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Failed to ensure database schema on startup");
            throw;
        }

        _consumer.Subscribe(_kafkaOptions.RawTopic);

        _logger.LogInformation(
            "Weather processor started: consuming {RawTopic} as group {GroupId}, publishing enriched readings to {ProcessedTopic}",
            _kafkaOptions.RawTopic, _kafkaOptions.ConsumerGroupId, _kafkaOptions.ProcessedTopic);

        while (!stoppingToken.IsCancellationRequested)
        {
            ConsumeResult<string, string>? consumeResult = null;
            WeatherReading? reading = null;
            string? correlationId = null;
            try
            {
                consumeResult = _consumer.Consume(stoppingToken);
                if (consumeResult?.Message is null)
                {
                    continue;
                }

                correlationId = CorrelationIdHeader.TryGet(consumeResult.Message.Headers) ?? Guid.NewGuid().ToString();
                var stopwatch = Stopwatch.StartNew();

                reading = JsonSerializer.Deserialize<WeatherReading>(consumeResult.Message.Value)
                    ?? throw new InvalidOperationException("Received an empty weather reading.");

                var enriched = _calculator.Enrich(reading);

                await _repository.InsertAsync(enriched, stoppingToken);

                var producedMessage = new Message<string, string>
                {
                    Key = enriched.StationId,
                    Value = JsonSerializer.Serialize(enriched),
                    Headers = new Headers()
                };
                CorrelationIdHeader.Set(producedMessage.Headers, correlationId);

                await _producer.ProduceAsync(_kafkaOptions.ProcessedTopic, producedMessage, stoppingToken);

                _consumer.Commit(consumeResult);

                stopwatch.Stop();
                var stationTag = new KeyValuePair<string, object?>("station_id", enriched.StationId);
                WeatherProcessorMetrics.ReadingsProcessed.Add(1, stationTag);
                WeatherProcessorMetrics.ProcessingDuration.Record(stopwatch.Elapsed.TotalSeconds, stationTag);

                _logger.LogInformation(
                    "Processed reading for station {StationId}: dew point {DewPoint:F1}F, heat index {HeatIndex:F1}F, correlation {CorrelationId}",
                    enriched.StationId, enriched.DewPoint, enriched.HeatIndex, correlationId);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Weather processor stopping");
                break;
            }
            catch (Exception ex)
            {
                WeatherProcessorMetrics.ProcessingFailures.Add(1);
                _logger.LogError(ex,
                    "Failed to process reading for station {StationId} at {TopicPartitionOffset}, correlation {CorrelationId}",
                    reading?.StationId, consumeResult?.TopicPartitionOffset, correlationId);
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
