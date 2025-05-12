using System;
using System.Collections.Generic;
using RuleEngine.Core;

namespace RuleEngine.Rules
{
    public class SimpleRule : RuleBase
    {
        private readonly Func<IDictionary<string, object>, bool> _condition;
        private readonly Func<IDictionary<string, object>, IDictionary<string, object>> _action;

        public SimpleRule(
            string ruleId,
            string ruleName,
            Func<IDictionary<string, object>, bool> condition,
            Func<IDictionary<string, object>, IDictionary<string, object>> action)
            : base(ruleId, ruleName)
        {
            _condition = condition ?? throw new ArgumentNullException(nameof(condition));
            _action = action ?? throw new ArgumentNullException(nameof(action));
        }

        public override bool Evaluate(IDictionary<string, object> inputs)
        {
            try
            {
                return _condition(inputs);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public override IDictionary<string, object> Execute(IDictionary<string, object> inputs)
        {
            try
            {
                return _action(inputs);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error executing rule {RuleId}: {ex.Message}", ex);
            }
        }
    }
} 