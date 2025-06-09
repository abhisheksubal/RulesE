using System;
using System.Collections.Generic;
using RuleEngine.Core;

namespace RuleEngine.Rules
{
    public enum LogicalOperator
    {
        And,
        Or,
        Not
    }

    public class CompositeRule : RuleBase
    {
        private readonly LogicalOperator _operator;
        private readonly List<IRule> _rules;
        private readonly IDictionary<string, ActionDefinition> _actions;

        public CompositeRule(
            string ruleId, 
            string ruleName, 
            LogicalOperator logicalOperator, 
            IEnumerable<IRule> rules,
            IDictionary<string, ActionDefinition>? actions = null) 
            : base(ruleId, ruleName)
        {
            _operator = logicalOperator;
            _rules = new List<IRule>();
            foreach (var rule in rules)
            {
                _rules.Add(rule);
            }
            
            if (_rules.Count == 0)
            {
                throw new ArgumentException("At least one rule must be provided for a composite rule");
            }
            
            // If using NOT operator, only one rule is allowed
            if (_operator == LogicalOperator.Not && _rules.Count != 1)
            {
                throw new ArgumentException("NOT operator can only be applied to a single rule");
            }
            
            _actions = actions ?? new Dictionary<string, ActionDefinition>();
        }

        public override bool Evaluate(IDictionary<string, object> inputs)
        {
            try
            {
                switch (_operator)
                {
                    case LogicalOperator.And:
                        foreach (var rule in _rules)
                        {
                            if (!rule.Evaluate(inputs))
                            {
                                return false;
                            }
                        }
                        return true;
                    
                    case LogicalOperator.Or:
                        foreach (var rule in _rules)
                        {
                            if (rule.Evaluate(inputs))
                            {
                                return true;
                            }
                        }
                        return false;
                    
                    case LogicalOperator.Not:
                        return !_rules[0].Evaluate(inputs);
                    
                    default:
                        throw new ArgumentException($"Unsupported logical operator: {_operator}");
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        public override IDictionary<string, object> Execute(IDictionary<string, object> inputs)
        {
            var results = new Dictionary<string, object>();
            
            // Execute all rules that evaluate to true and collect their results
            foreach (var rule in _rules)
            {
                if (rule.Evaluate(inputs))
                {
                    var ruleResults = rule.Execute(inputs);
                    foreach (var result in ruleResults)
                    {
                        results[result.Key] = result.Value;
                    }
                }
            }
            
            // Apply this composite rule's actions (if any)
            if (_actions != null)
            {
                foreach (var action in _actions)
                {
                    results[action.Key] = ExecuteAction(inputs, action.Key, action.Value);
                }
            }
            
            return results;
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
    
    // Action definition class to support composite rules
    public class ActionDefinition
    {
        public string? Operator { get; set; }
        public object? Value { get; set; }
    }
} 