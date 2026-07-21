using System.Text.Json;
using RulesEngine.Models;
using WeatherRules.Worker.Models;

namespace WeatherRules.Worker.Rules;

public class WeatherRuleEvaluator
{
    private const string WorkflowName = "WeatherAlertRules";
    private const string DefaultSeverity = "Info";

    private readonly RulesEngine.RulesEngine _engine;

    public WeatherRuleEvaluator(List<Workflow> workflows)
    {
        var reSettings = new ReSettings { CustomTypes = [typeof(StationLocation)] };
        _engine = new RulesEngine.RulesEngine(workflows.ToArray(), reSettings);
    }

    public async Task<IReadOnlyList<Alert>> EvaluateAsync(EnrichedWeatherReading reading, CancellationToken cancellationToken)
    {
        var ruleParameters = new[] { new RuleParameter("reading", reading) };
        var results = await _engine.ExecuteAllRulesAsync(WorkflowName, ruleParameters);

        var triggeredAt = DateTimeOffset.UtcNow;

        return results
            .Where(result => result.IsSuccess)
            .Select(result => new Alert(
                reading.StationId,
                result.Rule.RuleName,
                ExtractSeverity(result.Rule.Properties),
                result.Rule.ErrorMessage,
                reading.Timestamp,
                triggeredAt))
            .ToList();
    }

    private static string ExtractSeverity(Dictionary<string, object>? properties)
    {
        if (properties is null || !properties.TryGetValue("Severity", out var value))
        {
            return DefaultSeverity;
        }

        return value switch
        {
            JsonElement { ValueKind: JsonValueKind.String } element => element.GetString() ?? DefaultSeverity,
            string severity => severity,
            _ => DefaultSeverity
        };
    }
}
