using System.Collections.Generic;
using System.Linq;

namespace RuleEngine.Core
{
    public class RuleEngine
    {
        private readonly List<IRule> _rules;
        private readonly IRuleParser _ruleParser;
        private readonly OperatorRegistry _operatorRegistry;
        private readonly RuleFactoryRegistry _ruleFactoryRegistry;

        public RuleEngine(OperatorRegistry operatorRegistry, RuleFactoryRegistry ruleFactoryRegistry, IRuleParser? ruleParser = null)
        {
            _rules = new List<IRule>();
            _operatorRegistry = operatorRegistry;
            _ruleFactoryRegistry = ruleFactoryRegistry;
            _ruleParser = ruleParser ?? new JsonRuleParser(operatorRegistry, ruleFactoryRegistry);
        }

        public void AddRule(string ruleDefinition)
        {
            var rule = _ruleParser.Parse(ruleDefinition);
            _rules.Add(rule);
        }

        public void AddRule(IRule rule)
        {
            _rules.Add(rule);
        }

        public void RemoveRule(string ruleId)
        {
            _rules.RemoveAll(r => r.RuleId == ruleId);
        }

        public IDictionary<string, object> ExecuteRules(IDictionary<string, object> inputs)
        {
            var results = new Dictionary<string, object>();
            
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