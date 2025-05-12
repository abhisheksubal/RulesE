namespace RuleEngine.Core
{
    public interface IRuleParser
    {
        IRule Parse(string ruleDefinition);
    }
} 