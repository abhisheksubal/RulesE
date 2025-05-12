using System.Collections.Generic;

namespace RuleEngine.Core
{
    public abstract class RuleBase : IRule
    {
        public string RuleId { get; protected set; }
        public string RuleName { get; protected set; }

        protected RuleBase(string ruleId, string ruleName)
        {
            RuleId = ruleId;
            RuleName = ruleName;
        }

        public abstract bool Evaluate(IDictionary<string, object> inputs);
        public abstract IDictionary<string, object> Execute(IDictionary<string, object> inputs);
    }
} 