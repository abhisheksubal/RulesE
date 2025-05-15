using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using RuleEngine.Core;
using RuleEngine.Models;
using ModelActionDefinition = RuleEngine.Models.ActionDefinition;
using RuleActionDefinition = RuleEngine.Rules.ActionDefinition;

namespace RuleEngine.Rules.Factories
{
    /// <summary>
    /// Factory for creating composite rules
    /// </summary>
    public class CompositeRuleFactory : IRuleFactory
    {
        private readonly OperatorRegistry _operatorRegistry;
        private readonly RuleFactoryRegistry _ruleFactoryRegistry;
        
        /// <summary>
        /// Creates a new composite rule factory
        /// </summary>
        /// <param name="operatorRegistry">The operator registry for validation</param>
        /// <param name="ruleFactoryRegistry">The rule factory registry for creating child rules</param>
        public CompositeRuleFactory(OperatorRegistry operatorRegistry, RuleFactoryRegistry ruleFactoryRegistry)
        {
            _operatorRegistry = operatorRegistry ?? throw new ArgumentNullException(nameof(operatorRegistry));
            _ruleFactoryRegistry = ruleFactoryRegistry ?? throw new ArgumentNullException(nameof(ruleFactoryRegistry));
        }
        
        /// <summary>
        /// Gets the rule type this factory handles
        /// </summary>
        public string RuleType => "composite";
        
        /// <summary>
        /// Creates a composite rule from rule definition data
        /// </summary>
        /// <param name="ruleData">The rule definition data</param>
        /// <returns>A composite rule instance</returns>
        public IRule Create(RuleDefinition ruleData)
        {
            if (ruleData == null)
                throw new ArgumentNullException(nameof(ruleData));
                
            if (string.IsNullOrEmpty(ruleData.RuleId))
                throw new ArgumentException("RuleId is required for CompositeRule");
                
            if (string.IsNullOrEmpty(ruleData.RuleName))
                throw new ArgumentException("RuleName is required for CompositeRule");
                
            if (string.IsNullOrEmpty(ruleData.Operator))
                throw new ArgumentException("Operator is required for CompositeRule");
                
            if (ruleData.Rules == null || ruleData.Rules.Count == 0)
                throw new ArgumentException("At least one child rule is required for CompositeRule");
                
            // Parse the logical operator
            if (!Enum.TryParse<LogicalOperator>(ruleData.Operator, true, out var logicalOperator))
            {
                throw new ArgumentException($"Invalid logical operator: {ruleData.Operator}. " +
                                          $"Must be one of: {string.Join(", ", Enum.GetNames(typeof(LogicalOperator)))}");
            }
            
            // Parse child rules
            var childRules = new List<IRule>();
            foreach (var childRuleData in ruleData.Rules)
            {
                // Get the appropriate factory for the child rule type
                var factory = _ruleFactoryRegistry.GetFactory(childRuleData.Type ?? "simple");
                
                // Create the child rule
                var childRule = factory.Create(childRuleData);
                childRules.Add(childRule);
            }
            
            // Validate action operators if actions exist
            IDictionary<string, RuleActionDefinition>? compositeActions = null;
            if (ruleData.Actions != null && ruleData.Actions.Count > 0)
            {
                compositeActions = new Dictionary<string, RuleActionDefinition>();
                
                foreach (var action in ruleData.Actions)
                {
                    if (action.Value == null || string.IsNullOrEmpty(action.Value.Operator))
                        throw new ArgumentException($"Action '{action.Key}' has no operator");
                        
                    if (!_operatorRegistry.IsValidActionOperator(action.Value.Operator))
                        throw new ArgumentException($"Invalid action operator: {action.Value.Operator}");
                        
                    // Convert from model action definition to rule action definition
                    compositeActions[action.Key] = new RuleActionDefinition
                    {
                        Operator = action.Value.Operator,
                        Value = action.Value.Value
                    };
                }
            }
            
            // Create the composite rule
            return new CompositeRule(
                ruleData.RuleId,
                ruleData.RuleName,
                logicalOperator,
                childRules,
                compositeActions
            );
        }
    }
} 