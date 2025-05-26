using System;
using System.Collections.Generic;
using RuleEngine.Models;

namespace RuleEngine.Core
{
    /// <summary>
    /// Registry for rule factories that manages creation of different rule types
    /// </summary>
    public class RuleFactoryRegistry
    {
        private readonly Dictionary<string, IRuleFactory> _factories = new Dictionary<string, IRuleFactory>(StringComparer.OrdinalIgnoreCase);
        
        /// <summary>
        /// Registers a rule factory
        /// </summary>
        /// <param name="factory">The factory to register</param>
        public void RegisterFactory(IRuleFactory factory)
        {
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));
                
            _factories[factory.RuleType.ToLower()] = factory;
        }
        
        /// <summary>
        /// Gets a factory for the specified rule type
        /// </summary>
        /// <param name="ruleType">The rule type</param>
        /// <returns>The corresponding factory</returns>
        /// <exception cref="ArgumentException">Thrown when no factory exists for the rule type</exception>
        public IRuleFactory GetFactory(string ruleType)
        {
            if (string.IsNullOrEmpty(ruleType))
                throw new ArgumentException("Rule type cannot be null or empty", nameof(ruleType));
                
            if (_factories.TryGetValue(ruleType.ToLower(), out var factory))
                return factory;
                
            throw new ArgumentException($"Unsupported rule type: {ruleType}");
        }
        
        /// <summary>
        /// Checks if a factory exists for the specified rule type
        /// </summary>
        /// <param name="ruleType">The rule type to check</param>
        /// <returns>True if a factory exists, false otherwise</returns>
        public bool HasFactory(string ruleType)
        {
            if (string.IsNullOrEmpty(ruleType))
                return false;
                
            return _factories.ContainsKey(ruleType.ToLower());
        }

        /// <summary>
        /// Creates a rule from a rule definition
        /// </summary>
        /// <param name="ruleDefinition">The rule definition</param>
        /// <returns>The created rule</returns>
        /// <exception cref="ArgumentException">Thrown when the rule type is not supported</exception>
        public IRule CreateRule(RuleDefinition ruleDefinition)
        {
            if (ruleDefinition == null)
                throw new ArgumentNullException(nameof(ruleDefinition));

            if (string.IsNullOrEmpty(ruleDefinition.Type))
                throw new ArgumentException("Rule type is required", nameof(ruleDefinition));

            var factory = GetFactory(ruleDefinition.Type);
            return factory.Create(ruleDefinition);
        }
    }
} 