using System;
using System.Collections.Generic;
using RuleEngine.Core;
using Lua;
using Lua.Standard;

namespace RuleEngine.Rules
{
    public class LuaRule : RuleBase
    {
        private readonly string _conditionScript;
        private readonly Dictionary<string, string> _actionScripts;
        private readonly LuaState _luaState;

        public LuaRule(
            string ruleId,
            string ruleName,
            string conditionScript,
            Dictionary<string, string> actionScripts)
            : base(ruleId, ruleName)
        {
            _conditionScript = conditionScript ?? throw new ArgumentNullException(nameof(conditionScript));
            _actionScripts = actionScripts ?? throw new ArgumentNullException(nameof(actionScripts));
            _luaState = LuaState.Create();
            _luaState.OpenStandardLibraries();
            _luaState.OpenMathLibrary();
        }

        public override bool Evaluate(IDictionary<string, object> inputs)
        {
            _luaState.Environment.Clear();
            _luaState.OpenStandardLibraries();
            _luaState.OpenMathLibrary();
            try
            {
                // Set up the Lua environment with input values
                foreach (var input in inputs)
                {
                    _luaState.Environment[input.Key] = ConvertToLuaValue(input.Value);
                }

                // Execute the condition script
                var result = _luaState.DoStringAsync(_conditionScript).GetAwaiter().GetResult();
                var convertedResult = ConvertFromLuaValue(result[0]);
                return convertedResult != null && Convert.ToBoolean(convertedResult);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DEBUG] Exception during Lua evaluation: {ex.Message}");
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
                    return new Dictionary<string, object>(inputs); // Return copy of inputs if condition is not met
                }

                // Set up the Lua environment with input values
                _luaState.Environment.Clear();
                _luaState.OpenStandardLibraries();
                _luaState.OpenMathLibrary();
                foreach (var input in inputs)
                {
                    _luaState.Environment[input.Key] = ConvertToLuaValue(input.Value);
                }

                // Execute action scripts in order
                foreach (var actionScript in _actionScripts)
                {
                    // Check if this is a callback action
                    if (IsCallbackAction(actionScript.Value, out string callbackValue))
                    {
                        // Handle callback
                        if (!results.ContainsKey("__callbacks__"))
                        {
                            results["__callbacks__"] = new List<Dictionary<string, object>>();
                        }

                        // Evaluate the callback value against results
                        object evaluatedCallbackValue = callbackValue;
                        if (results.ContainsKey(callbackValue))
                        {
                            evaluatedCallbackValue = results[callbackValue];
                        }

                        var callbacks = (List<Dictionary<string, object>>)results["__callbacks__"];
                        callbacks.Add(new Dictionary<string, object>
                        {
                            { "name", actionScript.Key },
                            { "value", evaluatedCallbackValue }
                        });
                        continue;
                    }

                    // Execute the action script
                    var actionResult = _luaState.DoStringAsync(actionScript.Value).GetAwaiter().GetResult();
                    
                    // Add result to output and environment
                    var convertedResult = ConvertFromLuaValue(actionResult[0]);
                    results[actionScript.Key] = convertedResult;
                    _luaState.Environment[actionScript.Key] = ConvertToLuaValue(convertedResult);
                }

                return results;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error executing Lua rule: {ex.Message}", ex);
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

        private object? ConvertFromLuaValue(LuaValue value)
        {
            try
            {
                switch (value.Type)
                {
                    case LuaValueType.Nil:
                        return null;
                    case LuaValueType.Boolean:
                        return value.Read<bool>();
                    case LuaValueType.Number:
                        return value.Read<double>();
                    case LuaValueType.String:
                        return value.Read<string>();
                    case LuaValueType.Table:
                        return ConvertLuaTableToArray(value.Read<LuaTable>());
                    default:
                        return value.ToString();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DEBUG] Error converting Lua value: {ex.Message}");
                return null;
            }
        }

        private LuaValue ConvertToLuaValue(object value)
        {
            if (value == null)
                return LuaValue.Nil;

            return value switch
            {
                bool b => new LuaValue(b),
                int i => new LuaValue(i),
                double d => new LuaValue(d),
                string s => new LuaValue(s),
                Array array => ConvertArrayToLuaTable(array),
                _ => new LuaValue(value.ToString())
            };
        }

        private LuaValue ConvertArrayToLuaTable(Array array)
        {
            var table = new LuaTable();
            for (int i = 0; i < array.Length; i++)
            {
                table[i + 1] = ConvertToLuaValue(array.GetValue(i));
            }
            return new LuaValue(table);
        }

        private object[] ConvertLuaTableToArray(LuaTable table)
        {
            var result = new List<object>();
            for (int i = 1; i <= table.ArrayLength; i++)
            {
                result.Add(ConvertFromLuaValue(table[i]));
            }
            return result.ToArray();
        }
    }
} 