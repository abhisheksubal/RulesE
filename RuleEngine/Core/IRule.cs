using System.Collections.Generic;

namespace RuleEngine.Core
{
    public interface IRule
    {
        string RuleId { get; }
        string RuleName { get; }
        bool Evaluate(IDictionary<string, object> inputs);
        IDictionary<string, object> Execute(IDictionary<string, object> inputs);
    }
} 