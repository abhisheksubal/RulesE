using System;
using System.Collections.Generic;

namespace RuleEngine.Core
{
    /// <summary>
    /// Registry for valid condition and action operators
    /// </summary>
    public class OperatorRegistry
    {
        private readonly HashSet<string> _conditionOperators = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> _actionOperators = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        
        /// <summary>
        /// Creates a new operator registry with default operators
        /// </summary>
        public OperatorRegistry()
        {
            // Register standard condition operators
            RegisterConditionOperators("equals", "==", "notequals", "!=", 
                "greaterthan", ">", "lessthan", "<", 
                "greaterthanorequal", ">=", "lessthanorequal", "<=");
                
            // Register standard action operators
            RegisterActionOperators("set", "=", "add", "+=", "subtract", "-=",
                "multiply", "*=", "divide", "/=");
        }
        
        /// <summary>
        /// Registers condition operators
        /// </summary>
        /// <param name="operators">The operators to register</param>
        public void RegisterConditionOperators(params string[] operators)
        {
            if (operators == null)
                throw new ArgumentNullException(nameof(operators));
                
            foreach (var op in operators)
            {
                if (!string.IsNullOrEmpty(op))
                    _conditionOperators.Add(op);
            }
        }
        
        /// <summary>
        /// Registers action operators
        /// </summary>
        /// <param name="operators">The operators to register</param>
        public void RegisterActionOperators(params string[] operators)
        {
            if (operators == null)
                throw new ArgumentNullException(nameof(operators));
                
            foreach (var op in operators)
            {
                if (!string.IsNullOrEmpty(op))
                    _actionOperators.Add(op);
            }
        }
        
        /// <summary>
        /// Checks if a condition operator is valid
        /// </summary>
        /// <param name="op">The operator to check</param>
        /// <returns>True if valid, false otherwise</returns>
        public bool IsValidConditionOperator(string op)
        {
            return !string.IsNullOrEmpty(op) && _conditionOperators.Contains(op);
        }
        
        /// <summary>
        /// Checks if an action operator is valid
        /// </summary>
        /// <param name="op">The operator to check</param>
        /// <returns>True if valid, false otherwise</returns>
        public bool IsValidActionOperator(string op)
        {
            return !string.IsNullOrEmpty(op) && _actionOperators.Contains(op);
        }
        
        /// <summary>
        /// Gets all registered condition operators
        /// </summary>
        /// <returns>Array of condition operators</returns>
        public string[] GetConditionOperators()
        {
            string[] operators = new string[_conditionOperators.Count];
            _conditionOperators.CopyTo(operators);
            return operators;
        }
        
        /// <summary>
        /// Gets all registered action operators
        /// </summary>
        /// <returns>Array of action operators</returns>
        public string[] GetActionOperators()
        {
            string[] operators = new string[_actionOperators.Count];
            _actionOperators.CopyTo(operators);
            return operators;
        }
    }
} 