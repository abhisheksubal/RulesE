using System;
using System.Collections.Generic;

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
    }
} 