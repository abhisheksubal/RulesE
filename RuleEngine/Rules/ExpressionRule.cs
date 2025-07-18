using System;
using System.Collections.Generic;
using RuleEngine.Core;
// Import the NCalcExtensions namespace
using PanoramicData.NCalcExtensions;
// NCalc is still needed as ExtendedExpression inherits from it
using NCalc;

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

        /// <summary>
        /// Creates and configures an ExtendedExpression.
        /// </summary>
        /// <param name="expressionString">The expression to evaluate.</param>
        /// <param name="parameters">The parameters to use in the expression.</param>
        /// <returns>A configured ExtendedExpression instance.</returns>
        private ExtendedExpression CreateAndConfigureExpression(string expressionString, IDictionary<string, object> parameters)
        {
            // Use ExtendedExpression instead of NCalc.Expression
            var expression = new ExtendedExpression(expressionString);

            // Set parameters from the provided dictionary
            foreach (var param in parameters)
            {
                expression.Parameters[param.Key] = param.Value;
            }

            // Add custom function support
            expression.EvaluateFunction += (name, args) =>
            {
                // Custom function to check if a value is a number
                if (name.Equals("isNumber", StringComparison.OrdinalIgnoreCase))
                {
                    // Ensure there is exactly one parameter
                    if (args.Parameters.Length != 1)
                    {
                        throw new ArgumentException("isNumber() requires exactly one parameter.");
                    }
                    var value = args.Parameters[0].Evaluate();
                    // Try to parse the value as a double
                    args.Result = double.TryParse(value?.ToString(), out _);
                }
            };

            return expression;
        }

        public override bool Evaluate(IDictionary<string, object> inputs)
        {
            try
            {
                // var inputStrings = new List<string>();
                // foreach (var kv in inputs)
                // {
                //     inputStrings.Add($"{kv.Key}={kv.Value}");
                // }
                // Console.WriteLine($"[DEBUG] Evaluating condition: '{_conditionExpression}' with inputs: {string.Join(", ", inputStrings)}");
                
                var expression = CreateAndConfigureExpression(_conditionExpression, inputs);

                if(expression.HasErrors())
                {
                    // Log the error from the expression
                    Console.WriteLine($"[DEBUG] Expression has error: {expression.Error}");
                    return false;
                }

                var evalResult = expression.Evaluate();
                Console.WriteLine($"[DEBUG] Evaluation result: {evalResult}");
                return Convert.ToBoolean(evalResult);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DEBUG] Exception during evaluation: {ex.Message}");
                // It's often better to re-throw or handle specific exceptions
                // For this conversion, we'll keep the original behavior.
                return false;
            }
        }

        public override IDictionary<string, object> Execute(IDictionary<string, object> inputs)
        {
            if (inputs == null)
            {
                throw new ArgumentNullException(nameof(inputs));
            }

            var results = new Dictionary<string, object>();

            try
            {
                // First, evaluate the condition
                if (!Evaluate(inputs))
                {
                    return results; // Return empty results if the condition is not met
                }

                // If the condition is met, execute the actions
                foreach (var actionExpr in _actionExpressions)
                {
                    // Check if this is a callback action
                    if (IsCallbackAction(actionExpr.Value, out string callbackValue))
                    {
                        // Handle callback logic as before
                        if (!inputs.ContainsKey("__callbacks__"))
                        {
                            inputs["__callbacks__"] = new List<Dictionary<string, object>>();
                        }

                        // Evaluate the callback value against results
                        object evaluatedCallbackValue = callbackValue;
                        if (results.ContainsKey(callbackValue))
                        {
                            evaluatedCallbackValue = results[callbackValue];
                        }

                        var callbacks = (List<Dictionary<string, object>>)inputs["__callbacks__"];
                        callbacks.Add(new Dictionary<string, object>
                        {
                            { "name", actionExpr.Key },
                            { "value", evaluatedCallbackValue }
                        });
                        continue;
                    }

                    // For action expressions, parameters should include the initial inputs
                    // and any results from previous actions in this execution sequence.
                    var currentParameters = new Dictionary<string, object>(inputs);
                    foreach (var result in results)
                    {
                        currentParameters[result.Key] = result.Value;
                    }
                    
                    var expression = CreateAndConfigureExpression(actionExpr.Value, currentParameters);

                    if(expression.HasErrors())
                    {
                        throw new InvalidOperationException($"Error in action expression for '{actionExpr.Key}': {expression.Error}");
                    }
                    
                    // Evaluate the expression and store the result
                    var actionResult = expression.Evaluate();
                    results[actionExpr.Key] = actionResult;
                }

                // Copy __callbacks__ from inputs to results if it exists
                if (inputs.ContainsKey("__callbacks__"))
                {
                    results["__callbacks__"] = inputs["__callbacks__"];
                }

                return results;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error executing expression rule: {ex.Message}", ex);
            }
        }

        private bool IsCallbackAction(string actionValue, out string callbackValue)
        {
            callbackValue = null;
            
            // Check for arrow syntax (=>)
            if (actionValue.StartsWith("=>"))
            {
                callbackValue = actionValue.Substring(2).Trim();
                return true;
            }
            
            // Check for callback function syntax
            if (actionValue.StartsWith("callback(") && actionValue.EndsWith(")"))
            {
                callbackValue = actionValue.Substring(9, actionValue.Length - 10).Trim();
                return true;
            }
            
            return false;
        }
    }
}