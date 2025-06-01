using System.Collections.Generic;
using System.Linq;

namespace RuleEngine.Core
{
    public class RuleEngine
    {
        private readonly List<IRule> _rules;
        private readonly IRuleParser _ruleParser;

        public RuleEngine(IRuleParser ruleParser)
        {
            _rules = new List<IRule>();
            _ruleParser = ruleParser;
        }

        public void AddRule(string ruleDefinition)
        {
            var rule = _ruleParser.Parse(ruleDefinition);
            _rules.Add(rule);
        }

        public void RemoveRule(string ruleId)
        {
            _rules.RemoveAll(r => r.RuleId == ruleId);
        }

        public IDictionary<string, object> ExecuteRules(IDictionary<string, object> inputs)
        {
            // Initialize results with a copy of the input values
            var results = new Dictionary<string, object>(inputs);
            
            foreach (var rule in _rules)
            {
                if (rule.Evaluate(inputs))
                {
                    var ruleResults = rule.Execute(inputs);
                    foreach (var result in ruleResults)
                    {
                        results[result.Key] = result.Value;
                    }
                }
            }

            return results;
        }

        public IEnumerable<IRule> GetRules()
        {
            return _rules.ToList();
        }
    }
} 