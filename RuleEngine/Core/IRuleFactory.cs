using RuleEngine.Models;

namespace RuleEngine.Core
{
    /// <summary>
    /// Interface for rule factory implementations
    /// </summary>
    public interface IRuleFactory
    {
        /// <summary>
        /// The rule type this factory handles
        /// </summary>
        string RuleType { get; }
        
        /// <summary>
        /// Creates a rule instance from rule definition data
        /// </summary>
        /// <param name="ruleData">The rule definition data</param>
        /// <returns>An instantiated rule</returns>
        IRule Create(RuleDefinition ruleData);
    }
} 