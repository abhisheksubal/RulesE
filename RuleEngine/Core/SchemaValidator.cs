using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RuleEngine.Models;

namespace RuleEngine.Core
{
    /// <summary>
    /// Validates rule definitions
    /// </summary>
    public class RuleValidator
    {
        private readonly OperatorRegistry _operatorRegistry;
        
        /// <summary>
        /// Creates a new rule validator
        /// </summary>
        /// <param name="operatorRegistry">The operator registry for validating operators</param>
        public RuleValidator(OperatorRegistry operatorRegistry)
        {
            _operatorRegistry = operatorRegistry ?? throw new ArgumentNullException(nameof(operatorRegistry));
        }
        
        /// <summary>
        /// Validates a rule definition
        /// </summary>
        /// <param name="ruleJson">The rule definition JSON</param>
        /// <param name="errors">Output list of validation errors</param>
        /// <returns>True if valid, false otherwise</returns>
        public bool Validate(string ruleJson, out List<string> errors)
        {
            errors = new List<string>();
            
            if (string.IsNullOrEmpty(ruleJson))
            {
                errors.Add("Rule JSON cannot be null or empty");
                return false;
            }
            
            try
            {
                // Parse the JSON
                var ruleObj = JObject.Parse(ruleJson);
                
                // Get the rule type
                string ruleType = ruleObj["type"]?.ToString()?.ToLower() ?? "simple";
                
                // Validate common properties
                ValidateCommonProperties(ruleObj, errors);
                
                // Validate type-specific properties
                switch (ruleType)
                {
                    case "simple":
                        ValidateSimpleRule(ruleObj, errors);
                        break;
                    case "expression":
                        ValidateExpressionRule(ruleObj, errors);
                        break;
                    case "composite":
                        ValidateCompositeRule(ruleObj, errors);
                        break;
                    default:
                        errors.Add($"Unsupported rule type: {ruleType}");
                        break;
                }
                
                return errors.Count == 0;
            }
            catch (JsonException ex)
            {
                errors.Add($"Invalid JSON: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                errors.Add($"Validation error: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Validates a rule definition and throws if invalid
        /// </summary>
        /// <param name="ruleJson">The rule definition JSON</param>
        /// <exception cref="ValidationException">Thrown when validation fails</exception>
        public void ValidateAndThrow(string ruleJson)
        {
            List<string> errors;
            if (!Validate(ruleJson, out errors))
            {
                throw new ValidationException($"Invalid rule definition: {string.Join(", ", errors)}");
            }
        }
        
        /// <summary>
        /// Validates common properties for all rule types
        /// </summary>
        private void ValidateCommonProperties(JObject ruleObj, List<string> errors)
        {
            // Validate required properties
            if (string.IsNullOrEmpty(ruleObj["ruleId"]?.ToString()))
                errors.Add("ruleId is required");
                
            if (string.IsNullOrEmpty(ruleObj["ruleName"]?.ToString()))
                errors.Add("ruleName is required");
                
            if (string.IsNullOrEmpty(ruleObj["type"]?.ToString()))
                errors.Add("type is required");
        }
        
        /// <summary>
        /// Validates a simple rule
        /// </summary>
        private void ValidateSimpleRule(JObject ruleObj, List<string> errors)
        {
            // Validate conditions
            var conditions = ruleObj["conditions"] as JObject;
            if (conditions == null || !conditions.HasValues)
            {
                errors.Add("conditions is required and must have at least one condition");
                return;
            }
            
            // Validate each condition
            foreach (var property in conditions.Properties())
            {
                var condition = property.Value as JObject;
                if (condition == null)
                {
                    errors.Add($"Condition '{property.Name}' must be an object");
                    continue;
                }
                
                var operatorToken = condition["operator"];
                if (operatorToken == null)
                {
                    errors.Add($"Condition '{property.Name}' must have an operator");
                    continue;
                }
                
                string operatorValue = operatorToken.ToString();
                if (!_operatorRegistry.IsValidConditionOperator(operatorValue))
                {
                    errors.Add($"Invalid condition operator '{operatorValue}' in condition '{property.Name}'");
                }
            }
            
            // Validate actions
            var actions = ruleObj["actions"] as JObject;
            if (actions != null)
            {
                foreach (var property in actions.Properties())
                {
                    var action = property.Value as JObject;
                    if (action == null)
                    {
                        errors.Add($"Action '{property.Name}' must be an object");
                        continue;
                    }
                    
                    var operatorToken = action["operator"];
                    if (operatorToken == null)
                    {
                        errors.Add($"Action '{property.Name}' must have an operator");
                        continue;
                    }
                    
                    string operatorValue = operatorToken.ToString();
                    if (!_operatorRegistry.IsValidActionOperator(operatorValue))
                    {
                        errors.Add($"Invalid action operator '{operatorValue}' in action '{property.Name}'");
                    }
                }
            }
        }
        
        /// <summary>
        /// Validates an expression rule
        /// </summary>
        private void ValidateExpressionRule(JObject ruleObj, List<string> errors)
        {
            // Validate condition expression
            var conditionExpression = ruleObj["conditionExpression"]?.ToString();
            if (string.IsNullOrEmpty(conditionExpression))
            {
                errors.Add("conditionExpression is required");
            }
            
            // Validate action expressions
            var actionExpressions = ruleObj["actionExpressions"] as JObject;
            if (actionExpressions == null || !actionExpressions.HasValues)
            {
                errors.Add("actionExpressions is required and must have at least one expression");
                return;
            }
            
            // Validate each action expression
            foreach (var property in actionExpressions.Properties())
            {
                if (string.IsNullOrEmpty(property.Value?.ToString()))
                {
                    errors.Add($"Action expression '{property.Name}' cannot be empty");
                }
            }
        }
        
        /// <summary>
        /// Validates a composite rule
        /// </summary>
        private void ValidateCompositeRule(JObject ruleObj, List<string> errors)
        {
            // Validate operator
            var operatorValue = ruleObj["operator"]?.ToString();
            if (string.IsNullOrEmpty(operatorValue))
            {
                errors.Add("operator is required");
            }
            else
            {
                // Check if it's a valid logical operator
                if (!Enum.TryParse<Rules.LogicalOperator>(operatorValue, true, out _))
                {
                    errors.Add($"Invalid logical operator: {operatorValue}. Must be one of: And, Or, Not");
                }
            }
            
            // Validate rules
            var rules = ruleObj["rules"] as JArray;
            if (rules == null || rules.Count == 0)
            {
                errors.Add("rules is required and must have at least one rule");
                return;
            }
            
            // Check NOT operator has exactly one rule
            if (operatorValue?.Equals("Not", StringComparison.OrdinalIgnoreCase) == true && rules.Count != 1)
            {
                errors.Add("NOT operator can only be applied to a single rule");
            }
            
            // Validate actions
            var actions = ruleObj["actions"] as JObject;
            if (actions != null)
            {
                foreach (var property in actions.Properties())
                {
                    var action = property.Value as JObject;
                    if (action == null)
                    {
                        errors.Add($"Action '{property.Name}' must be an object");
                        continue;
                    }
                    
                    var actionOperator = action["operator"]?.ToString();
                    if (string.IsNullOrEmpty(actionOperator))
                    {
                        errors.Add($"Action '{property.Name}' must have an operator");
                        continue;
                    }
                    
                    if (!_operatorRegistry.IsValidActionOperator(actionOperator))
                    {
                        errors.Add($"Invalid action operator '{actionOperator}' in action '{property.Name}'");
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// Exception thrown when rule validation fails
    /// </summary>
    public class ValidationException : Exception
    {
        public ValidationException(string message) : base(message) { }
        public ValidationException(string message, Exception innerException) : base(message, innerException) { }
    }
} 