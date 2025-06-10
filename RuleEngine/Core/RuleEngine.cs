using System.Collections.Generic;

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
            for (int i = _rules.Count - 1; i >= 0; i--)
            {
                if (_rules[i].RuleId == ruleId)
                {
                    _rules.RemoveAt(i);
                }
            }
        }

        public IDictionary<string, object> ExecuteRules(IDictionary<string, object> inputs)
        {
            // Initialize results with a copy of the input values
            var results = new Dictionary<string, object>(inputs);
            
            foreach (var rule in _rules)
            {
                if (rule.Evaluate(results))
                {
                    var ruleResults = rule.Execute(results);
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
            var rulesCopy = new List<IRule>(_rules.Count);
            foreach (var rule in _rules)
            {
                rulesCopy.Add(rule);
            }
            return rulesCopy;
        }
    }
} 