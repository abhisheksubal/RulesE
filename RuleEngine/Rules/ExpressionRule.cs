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

        private Expression CreateExpression(string expression)
        {
            var expr = new Expression(expression);
            expr.EvaluateFunction += (name, args) =>
            {
                Console.WriteLine($"Function: {name}, Parameters: {string.Join(", ", args.Parameters.Select(p => p == null ? "null" : p.ToString()))}");
                if (name.ToLower() == "isnull")
                {
                    var evaluatedValue = args.Parameters[0].Evaluate();
                    Console.WriteLine($"Evaluated value for IsNull: {evaluatedValue}");
                }
                switch (name.ToLower())
                {
                    case "isnull":
                        args.Result = args.Parameters[0] == null || object.Equals(args.Parameters[0], DBNull.Value) || string.IsNullOrEmpty(args.Parameters[0].ToString());
                        break;
                    case "nvl":
                        args.Result = (args.Parameters[0] == null || object.Equals(args.Parameters[0], DBNull.Value) || string.IsNullOrEmpty(args.Parameters[0].ToString())) ? args.Parameters[1] : args.Parameters[0];
                        break;
                }
            };
            return expr;
        }

        public override bool Evaluate(IDictionary<string, object> inputs)
        {
            try
            {
                var expression = CreateExpression(_conditionExpression);

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
                    // Check if this is a callback action
                    if (IsCallbackAction(actionExpr.Value, out string callbackValue))
                    {
                        // Handle callback
                        if (!inputs.ContainsKey("__callbacks__"))
                        {
                            inputs["__callbacks__"] = new List<Dictionary<string, object>>();
                        }

                        var callbacks = (List<Dictionary<string, object>>)inputs["__callbacks__"];
                        callbacks.Add(new Dictionary<string, object>
                        {
                            { "name", actionExpr.Key },
                            { "value", callbackValue }
                        });

                        // Copy callbacks to results
                        results["__callbacks__"] = callbacks;
                        continue;
                    }

                    var expression = CreateExpression(actionExpr.Value);

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

                // Copy __callbacks__ from inputs to results if it exists
                if (inputs.ContainsKey("__callbacks__"))
                {
                    results["__callbacks__"] = inputs["__callbacks__"];
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error executing expressions: {ex.Message}", ex);
            }

            return results;
        }

        private bool IsCallbackAction(string actionValue, out string callbackValue)
        {
            callbackValue = null;

            // Check for => operator
            if (actionValue.StartsWith("=>"))
            {
                callbackValue = actionValue.Substring(2).Trim();
                return true;
            }

            // Check for callback operator
            if (actionValue.StartsWith("callback(") && actionValue.EndsWith(")"))
            {
                callbackValue = actionValue.Substring(9, actionValue.Length - 10).Trim();
                return true;
            }

            return false;
        }
    }
} 