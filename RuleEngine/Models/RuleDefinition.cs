using System.Collections.Generic;
using Newtonsoft.Json;

namespace RuleEngine.Models
{
    public class RuleDefinition
    {
        [JsonProperty("ruleId")]
        public string? RuleId { get; set; }

        [JsonProperty("ruleName")]
        public string? RuleName { get; set; }

        [JsonProperty("type")]
        public string? Type { get; set; }

        [JsonProperty("conditions")]
        public Dictionary<string, ConditionDefinition>? Conditions { get; set; }

        [JsonProperty("actions")]
        public Dictionary<string, ActionDefinition>? Actions { get; set; }
    }

    public class ConditionDefinition
    {
        [JsonProperty("operator")]
        public string? Operator { get; set; }

        [JsonProperty("value")]
        public object? Value { get; set; }
    }

    public class ActionDefinition
    {
        [JsonProperty("operator")]
        public string? Operator { get; set; }

        [JsonProperty("value")]
        public object? Value { get; set; }
    }
} 