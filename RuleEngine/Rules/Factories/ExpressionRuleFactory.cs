using System;
using System.Collections.Generic;
using RuleEngine.Core;
using RuleEngine.Models;

namespace RuleEngine.Rules.Factories
{
    /// <summary>
    /// Factory for creating expression rules
    /// </summary>
    public class ExpressionRuleFactory : IRuleFactory
    {
        /// <summary>
        /// Gets the rule type this factory handles
        /// </summary>
        public string RuleType => "expression";
        
        /// <summary>
        /// Creates an expression rule from rule definition data
        /// </summary>
        /// <param name="ruleData">The rule definition data</param>
        /// <returns>An expression rule instance</returns>
        public IRule Create(RuleDefinition ruleData)
        {
            if (ruleData == null)
                throw new ArgumentNullException(nameof(ruleData));
                
            if (string.IsNullOrEmpty(ruleData.RuleId))
                throw new ArgumentException("RuleId is required for ExpressionRule");
                
            if (string.IsNullOrEmpty(ruleData.RuleName))
                throw new ArgumentException("RuleName is required for ExpressionRule");
                
            if (string.IsNullOrEmpty(ruleData.ConditionExpression))
                throw new ArgumentException("ConditionExpression is required for ExpressionRule");
                
            if (ruleData.ActionExpressions == null || ruleData.ActionExpressions.Count == 0)
                throw new ArgumentException("At least one ActionExpression is required for ExpressionRule");
                
            return new ExpressionRule(
                ruleData.RuleId,
                ruleData.RuleName,
                ruleData.ConditionExpression,
                ruleData.ActionExpressions
            );
        }
    }
} 