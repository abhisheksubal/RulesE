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

        // For simple rules
        [JsonProperty("conditions")]
        public Dictionary<string, ConditionDefinition>? Conditions { get; set; }

        [JsonProperty("actions")]
        public Dictionary<string, ActionDefinition>? Actions { get; set; }

        // For expression rules
        [JsonProperty("conditionExpression")]
        public string? ConditionExpression { get; set; }

        [JsonProperty("actionExpressions")]
        public Dictionary<string, string>? ActionExpressions { get; set; }
        
        // For composite rules
        [JsonProperty("operator")]
        public string? Operator { get; set; }
        
        [JsonProperty("rules")]
        public List<RuleDefinition>? Rules { get; set; }
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