using System;
using System.Collections.Generic;
using RuleEngine.Core;
using RuleEngine.Models;

namespace RuleEngine.Rules.Factories
{
    /// <summary>
    /// Factory for creating Lua rules
    /// </summary>
    public class LuaRuleFactory : IRuleFactory
    {
        /// <summary>
        /// Gets the rule type this factory handles
        /// </summary>
        public string RuleType => "lua";
        
        /// <summary>
        /// Creates a Lua rule from rule definition data
        /// </summary>
        /// <param name="ruleData">The rule definition data</param>
        /// <returns>A Lua rule instance</returns>
        public IRule Create(RuleDefinition ruleData)
        {
            if (ruleData == null)
                throw new ArgumentNullException(nameof(ruleData));
                
            if (string.IsNullOrEmpty(ruleData.RuleId))
                throw new ArgumentException("RuleId is required for LuaRule");
                
            if (string.IsNullOrEmpty(ruleData.RuleName))
                throw new ArgumentException("RuleName is required for LuaRule");
                
            if (string.IsNullOrEmpty(ruleData.ConditionExpression))
                throw new ArgumentException("ConditionExpression is required for LuaRule");
                
            if (ruleData.ActionExpressions == null || ruleData.ActionExpressions.Count == 0)
                throw new ArgumentException("At least one ActionExpression is required for LuaRule");
                
            return new LuaRule(
                ruleData.RuleId,
                ruleData.RuleName,
                ruleData.ConditionExpression,
                ruleData.ActionExpressions
            );
        }
    }
} 