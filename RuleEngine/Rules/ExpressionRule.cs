using System;
using System.Collections.Generic;
using System.Linq;
using NCalc;
using RuleEngine.Core;

namespace RuleEngine.Rules
{
    public class ExpressionRule : RuleBase
    {
        private readonly string _conditionExpression;
        private readonly Dictionary<string, string> _actionExpressions;

        public ExpressionRule(
            string ruleId,
            string ruleName,
            string conditionExpression,
            Dictionary<string, string> actionExpressions)
            : base(ruleId, ruleName)
        {
            _conditionExpression = conditionExpression ?? throw new ArgumentNullException(nameof(conditionExpression));
            _actionExpressions = actionExpressions ?? throw new ArgumentNullException(nameof(actionExpressions));
        }

        public override bool Evaluate(IDictionary<string, object> inputs)
        {
            try
            {
                Expression expression = new Expression(_conditionExpression);

                // Set parameters from inputs
                foreach (var input in inputs)
                {
                    expression.Parameters[input.Key] = input.Value;
                }

                // Evaluate the expression
                var result = expression.Evaluate();

                // Convert result to boolean
                return Convert.ToBoolean(result);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public override IDictionary<string, object> Execute(IDictionary<string, object> inputs)
        {
            var results = new Dictionary<string, object>();

            try
            {
                foreach (var actionExpr in _actionExpressions)
                {
                    var expression = new Expression(actionExpr.Value);

                    // Set parameters from inputs
                    foreach (var input in inputs)
                    {
                        expression.Parameters[input.Key] = input.Value;
                    }

                    // Include the other results in parameters
                    foreach (var result in results)
                    {
                        expression.Parameters[result.Key] = result.Value;
                    }

                    // Evaluate the expression
                    var actionResult = expression.Evaluate();
                    
                    // Add result to output
                    results[actionExpr.Key] = actionResult;
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error executing expressions: {ex.Message}", ex);
            }

            return results;
        }
    }
} 