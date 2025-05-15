using System;
using System.Collections.Generic;
using RuleEngine.Core;
using RuleEngine.Models;
using ModelActionDefinition = RuleEngine.Models.ActionDefinition;
using RuleActionDefinition = RuleEngine.Rules.ActionDefinition;

namespace RuleEngine.Rules.Factories
{
    /// <summary>
    /// Factory for creating simple rules
    /// </summary>
    public class SimpleRuleFactory : IRuleFactory
    {
        private readonly OperatorRegistry _operatorRegistry;
        
        /// <summary>
        /// Creates a new simple rule factory
        /// </summary>
        /// <param name="operatorRegistry">The operator registry for validation</param>
        public SimpleRuleFactory(OperatorRegistry operatorRegistry)
        {
            _operatorRegistry = operatorRegistry ?? throw new ArgumentNullException(nameof(operatorRegistry));
        }
        
        /// <summary>
        /// Gets the rule type this factory handles
        /// </summary>
        public string RuleType => "simple";
        
        /// <summary>
        /// Creates a simple rule from rule definition data
        /// </summary>
        /// <param name="ruleData">The rule definition data</param>
        /// <returns>A simple rule instance</returns>
        public IRule Create(RuleDefinition ruleData)
        {
            if (ruleData == null)
                throw new ArgumentNullException(nameof(ruleData));
                
            if (string.IsNullOrEmpty(ruleData.RuleId))
                throw new ArgumentException("RuleId is required for SimpleRule");
                
            if (string.IsNullOrEmpty(ruleData.RuleName))
                throw new ArgumentException("RuleName is required for SimpleRule");
                
            if (ruleData.Conditions == null || ruleData.Conditions.Count == 0)
                throw new ArgumentException("At least one condition is required for SimpleRule");
                
            // Validate condition operators
            foreach (var condition in ruleData.Conditions)
            {
                if (condition.Value == null || string.IsNullOrEmpty(condition.Value.Operator))
                    throw new ArgumentException($"Condition '{condition.Key}' has no operator");
                    
                if (!_operatorRegistry.IsValidConditionOperator(condition.Value.Operator))
                    throw new ArgumentException($"Invalid condition operator: {condition.Value.Operator}");
            }
            
            // Validate action operators if actions exist
            if (ruleData.Actions != null)
            {
                foreach (var action in ruleData.Actions)
                {
                    if (action.Value == null || string.IsNullOrEmpty(action.Value.Operator))
                        throw new ArgumentException($"Action '{action.Key}' has no operator");
                        
                    if (!_operatorRegistry.IsValidActionOperator(action.Value.Operator))
                        throw new ArgumentException($"Invalid action operator: {action.Value.Operator}");
                }
            }
            
            // Create condition evaluator
            Func<IDictionary<string, object>, bool> conditionEvaluator = CreateConditionEvaluator(ruleData.Conditions);
            
            // Create action executor
            Func<IDictionary<string, object>, IDictionary<string, object>> actionExecutor = 
                inputs => ExecuteActions(inputs, ruleData.Actions ?? new Dictionary<string, ModelActionDefinition>());
                
            return new SimpleRule(
                ruleData.RuleId,
                ruleData.RuleName,
                conditionEvaluator,
                actionExecutor
            );
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
                string op = condition.Operator?.ToLower() ?? "";
                
                // Handle shorthand operators
                switch (op)
                {
                    case "==": op = "equals"; break;
                    case "!=": op = "notequals"; break;
                    case ">": op = "greaterthan"; break;
                    case "<": op = "lessthan"; break;
                    case ">=": op = "greaterthanorequal"; break;
                    case "<=": op = "lessthanorequal"; break;
                }
                
                switch (op)
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
        
        private int CompareValues(object value1, object value2)
        {
            if (value1 == null && value2 == null)
                return 0;
                
            if (value1 == null)
                return -1;
                
            if (value2 == null)
                return 1;
                
            if (value1 is IComparable comparable1 && value1.GetType() == value2.GetType())
                return comparable1.CompareTo(value2);
                
            // Try to convert to double for numeric comparison
            if (double.TryParse(value1.ToString(), out double num1) && 
                double.TryParse(value2.ToString(), out double num2))
                return num1.CompareTo(num2);
                
            // Default to string comparison
            return string.Compare(value1.ToString(), value2.ToString(), StringComparison.OrdinalIgnoreCase);
        }
        
        private IDictionary<string, object> ExecuteActions(
            IDictionary<string, object> inputs,
            Dictionary<string, ModelActionDefinition> actions)
        {
            var results = new Dictionary<string, object>();
            
            foreach (var action in actions)
            {
                try
                {
                    results[action.Key] = ExecuteAction(inputs, action.Key, action.Value);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Error executing action '{action.Key}': {ex.Message}", ex);
                }
            }
            
            return results;
        }
        
        private object ExecuteAction(IDictionary<string, object> inputs, string actionKey, ModelActionDefinition action)
        {
            string op = action.Operator?.ToLower() ?? "";
            
            // Handle shorthand operators
            switch (op)
            {
                case "=": op = "set"; break;
                case "+=": op = "add"; break;
                case "-=": op = "subtract"; break;
                case "*=": op = "multiply"; break;
                case "/=": op = "divide"; break;
            }
            
            // Handle value references (if action.Value is a string that refers to another input)
            object actionValue = action.Value;
            if (actionValue is string valueStr && inputs.ContainsKey(valueStr))
            {
                actionValue = inputs[valueStr];
            }
            
            switch (op)
            {
                case "set":
                    return actionValue;
                case "add":
                    return AddValues(inputs, actionKey, actionValue);
                case "subtract":
                    return SubtractValues(inputs, actionKey, actionValue);
                case "multiply":
                    return MultiplyValues(inputs, actionKey, actionValue);
                case "divide":
                    return DivideValues(inputs, actionKey, actionValue);
                default:
                    throw new ArgumentException($"Unsupported action operator: {action.Operator}");
            }
        }
        
        private object AddValues(IDictionary<string, object> inputs, string actionKey, object actionValue)
        {
            if (inputs.TryGetValue(actionKey, out var currentValue))
            {
                if (TryConvertToDouble(currentValue, out double currentNum) && 
                    TryConvertToDouble(actionValue, out double valueNum))
                {
                    return currentNum + valueNum;
                }
                
                // String concatenation
                return currentValue.ToString() + actionValue.ToString();
            }
            
            return actionValue;
        }
        
        private object SubtractValues(IDictionary<string, object> inputs, string actionKey, object actionValue)
        {
            if (inputs.TryGetValue(actionKey, out var currentValue) &&
                TryConvertToDouble(currentValue, out double currentNum) && 
                TryConvertToDouble(actionValue, out double valueNum))
            {
                return currentNum - valueNum;
            }
            
            throw new ArgumentException("Subtract operation requires numeric values");
        }
        
        private object MultiplyValues(IDictionary<string, object> inputs, string actionKey, object actionValue)
        {
            if (inputs.TryGetValue(actionKey, out var currentValue) &&
                TryConvertToDouble(currentValue, out double currentNum) && 
                TryConvertToDouble(actionValue, out double valueNum))
            {
                return currentNum * valueNum;
            }
            
            throw new ArgumentException("Multiply operation requires numeric values");
        }
        
        private object DivideValues(IDictionary<string, object> inputs, string actionKey, object actionValue)
        {
            if (inputs.TryGetValue(actionKey, out var currentValue) &&
                TryConvertToDouble(currentValue, out double currentNum) && 
                TryConvertToDouble(actionValue, out double valueNum))
            {
                if (valueNum == 0)
                    throw new DivideByZeroException("Cannot divide by zero");
                    
                return currentNum / valueNum;
            }
            
            throw new ArgumentException("Divide operation requires numeric values");
        }
        
        private bool TryConvertToDouble(object value, out double result)
        {
            result = 0;
            
            if (value == null)
                return false;
                
            if (value is double d)
            {
                result = d;
                return true;
            }
            
            if (value is int i)
            {
                result = i;
                return true;
            }
            
            if (value is float f)
            {
                result = f;
                return true;
            }
            
            return double.TryParse(value.ToString(), out result);
        }
    }
} 