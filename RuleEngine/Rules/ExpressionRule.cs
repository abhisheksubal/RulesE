using System;
using System.Collections.Generic;
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
                Console.WriteLine($"[DEBUG] Evaluating condition: '{_conditionExpression}' with inputs: {string.Join(", ", inputs)}");
                Expression expression = new Expression(_conditionExpression);

                // Set parameters from inputs
                foreach (var input in inputs)
                {
                    expression.Parameters[input.Key] = input.Value;
                }

                // Add ArrayGet, IsNull, and Nvl support
                expression.EvaluateFunction += (name, args) =>
                {
                    if (name == "ArrayGet")
                    {
                        var arrayObj = args.Parameters[0].Evaluate();
                        var index = Convert.ToInt32(args.Parameters[1].Evaluate());
                        object result = null;
                        if (arrayObj is Array arr)
                        {
                            result = arr.GetValue(index);
                        }
                        else if (arrayObj is object[] objArr)
                        {
                            result = objArr[index];
                        }
                        else if (arrayObj is System.Collections.IEnumerable genEnum)
                        {
                            var enumerator = genEnum.GetEnumerator();
                            int i = 0;
                            while (enumerator.MoveNext())
                            {
                                if (i == index)
                                {
                                    result = enumerator.Current;
                                    break;
                                }
                                i++;
                            }
                        }
                        args.Result = result;
                    }
                    else if (name == "IsNull")
                    {
                        var value = args.Parameters[0].Evaluate();
                        args.Result = value == null;
                    }
                    else if (name == "Nvl")
                    {
                        var value = args.Parameters[0].Evaluate();
                        var defaultValue = args.Parameters[1].Evaluate();
                        args.Result = value ?? defaultValue;
                    }
                };

                var evalResult = expression.Evaluate();
                Console.WriteLine($"[DEBUG] Evaluation result: {evalResult}");
                return Convert.ToBoolean(evalResult);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DEBUG] Exception during evaluation: {ex.Message}");
                return false;
            }
        }

        public override IDictionary<string, object> Execute(IDictionary<string, object> inputs)
        {
            if (inputs == null)
                throw new ArgumentNullException(nameof(inputs));

            var results = new Dictionary<string, object>();

            try
            {
                // First evaluate the condition
                if (!Evaluate(inputs))
                {
                    return results; // Return empty results if condition is not met
                }

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

                        // Copy callbacks to results
                        results["__callbacks__"] = callbacks;
                        continue;
                    }

                    var expression = new NCalc.Expression(actionExpr.Value);

                    // Set parameters from inputs
                    foreach (var input in inputs)
                    {
                        expression.Parameters[input.Key] = input.Value;
                    }

                    // Add ArrayGet, IsNull, and Nvl support
                    expression.EvaluateFunction += (name, args) =>
                    {
                        if (name == "ArrayGet")
                        {
                            var arrayObj = args.Parameters[0].Evaluate();
                            var index = Convert.ToInt32(args.Parameters[1].Evaluate());
                            object result = null;
                            if (arrayObj is Array arr)
                            {
                                result = arr.GetValue(index);
                            }
                            else if (arrayObj is object[] objArr)
                            {
                                result = objArr[index];
                            }
                            else if (arrayObj is System.Collections.IEnumerable genEnum)
                            {
                                var enumerator = genEnum.GetEnumerator();
                                int i = 0;
                                while (enumerator.MoveNext())
                                {
                                    if (i == index)
                                    {
                                        result = enumerator.Current;
                                        break;
                                    }
                                    i++;
                                }
                            }
                            args.Result = result;
                        }
                        else if (name == "IsNull")
                        {
                            var value = args.Parameters[0].Evaluate();
                            args.Result = value == null;
                        }
                        else if (name == "Nvl")
                        {
                            var value = args.Parameters[0].Evaluate();
                            var defaultValue = args.Parameters[1].Evaluate();
                            args.Result = value ?? defaultValue;
                        }
                    };

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