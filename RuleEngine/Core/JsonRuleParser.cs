using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using RuleEngine.Models;
using RuleEngine.Rules;

namespace RuleEngine.Core
{
    public class JsonRuleParser : IRuleParser
    {
        public IRule Parse(string ruleDefinition)
        {
            try
            {
                var ruleData = JsonConvert.DeserializeObject<RuleDefinition>(ruleDefinition);
                
                if (ruleData == null)
                {
                    throw new ArgumentException("Failed to deserialize rule definition");
                }

                if (string.IsNullOrEmpty(ruleData.RuleId) || string.IsNullOrEmpty(ruleData.RuleName))
                {
                    throw new ArgumentException("RuleId and RuleName are required");
                }

                switch (ruleData.Type?.ToLower())
                {
                    case "simple":
                        return CreateSimpleRule(ruleData);
                    case "composite":
                        return CreateCompositeRule(ruleData);
                    default:
                        throw new ArgumentException($"Unsupported rule type: {ruleData.Type}");
                }
            }
            catch (JsonException ex)
            {
                throw new ArgumentException("Invalid JSON rule definition", ex);
            }
        }

        private IRule CreateSimpleRule(RuleDefinition ruleData)
        {
            if (ruleData.Conditions == null || ruleData.Actions == null)
            {
                throw new ArgumentException("Conditions and Actions are required for SimpleRule");
            }

            return new SimpleRule(
                ruleData.RuleId!,
                ruleData.RuleName!,
                CreateConditionEvaluator(ruleData.Conditions),
                CreateActionExecutor(ruleData.Actions)
            );
        }

        private IRule CreateCompositeRule(RuleDefinition ruleData)
        {
            // Implementation for composite rules
            throw new NotImplementedException("Composite rules not yet implemented");
        }

        private Func<IDictionary<string, object>, bool> CreateConditionEvaluator(
            Dictionary<string, ConditionDefinition> conditions)
        {
            return inputs =>
            {
                foreach (var condition in conditions)
                {
                    if (!inputs.TryGetValue(condition.Key, out var inputValue))
                        return false;

                    var result = EvaluateCondition(inputValue, condition.Value);
                    if (!result)
                        return false;
                }
                return true;
            };
        }

        private bool EvaluateCondition(object inputValue, ConditionDefinition condition)
        {
            try
            {
                switch (condition.Operator?.ToLower())
                {
                    case "equals":
                        return Equals(inputValue, condition.Value);
                    case "notequals":
                        return !Equals(inputValue, condition.Value);
                    case "greaterthan":
                        return CompareValues(inputValue, condition.Value) > 0;
                    case "lessthan":
                        return CompareValues(inputValue, condition.Value) < 0;
                    case "greaterthanorequal":
                        return CompareValues(inputValue, condition.Value) >= 0;
                    case "lessthanorequal":
                        return CompareValues(inputValue, condition.Value) <= 0;
                    default:
                        throw new ArgumentException($"Unsupported condition operator: {condition.Operator}");
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        private int CompareValues(object? value1, object? value2)
        {
            try
            {
                // Try to convert both values to double for numeric comparison
                if (value1 != null && value2 != null)
                {
                    // Special case for numeric comparisons
                    if (double.TryParse(value1.ToString(), out double num1) && 
                        double.TryParse(value2.ToString(), out double num2))
                    {
                        return num1.CompareTo(num2);
                    }
                }
                
                if (value1 is IComparable comparable1 && value2 is IComparable comparable2)
                {
                    return comparable1.CompareTo(comparable2);
                }
                
                throw new ArgumentException("Values must implement IComparable");
            }
            catch (Exception)
            {
                throw;
            }
        }

        private Func<IDictionary<string, object>, IDictionary<string, object>> CreateActionExecutor(
            Dictionary<string, ActionDefinition> actions)
        {
            return inputs =>
            {
                var results = new Dictionary<string, object>();
                foreach (var action in actions)
                {
                    results[action.Key] = ExecuteAction(inputs, action.Key, action.Value);
                }
                return results;
            };
        }

        private object ExecuteAction(IDictionary<string, object> inputs, string actionKey, ActionDefinition action)
        {
            switch (action.Operator?.ToLower())
            {
                case "set":
                    return action.Value!;
                case "add":
                    return AddValues(inputs, actionKey, action);
                case "subtract":
                    return SubtractValues(inputs, actionKey, action);
                case "multiply":
                    return MultiplyValues(inputs, actionKey, action);
                case "divide":
                    return DivideValues(inputs, actionKey, action);
                default:
                    throw new ArgumentException($"Unsupported action operator: {action.Operator}");
            }
        }

        private object AddValues(IDictionary<string, object> inputs, string actionKey, ActionDefinition action)
        {
            if (action.Value is double numericValue && inputs.ContainsKey(actionKey))
            {
                return Convert.ToDouble(inputs[actionKey]) + numericValue;
            }
            throw new ArgumentException("Add operation requires numeric values");
        }

        private object SubtractValues(IDictionary<string, object> inputs, string actionKey, ActionDefinition action)
        {
            if (action.Value is double numericValue && inputs.ContainsKey(actionKey))
            {
                return Convert.ToDouble(inputs[actionKey]) - numericValue;
            }
            throw new ArgumentException("Subtract operation requires numeric values");
        }

        private object MultiplyValues(IDictionary<string, object> inputs, string actionKey, ActionDefinition action)
        {
            if (action.Value is double numericValue && inputs.ContainsKey(actionKey))
            {
                return Convert.ToDouble(inputs[actionKey]) * numericValue;
            }
            throw new ArgumentException("Multiply operation requires numeric values");
        }

        private object DivideValues(IDictionary<string, object> inputs, string actionKey, ActionDefinition action)
        {
            if (action.Value is double numericValue && inputs.ContainsKey(actionKey))
            {
                if (numericValue == 0)
                    throw new DivideByZeroException();
                return Convert.ToDouble(inputs[actionKey]) / numericValue;
            }
            throw new ArgumentException("Divide operation requires numeric values");
        }
    }
} 