# Rule Engine Operators

This document explains the operators supported by the Rule Engine and how to validate them.

## Table of Contents
- [Condition Operators](#condition-operators)
- [Action Operators](#action-operators)
- [Operator Validation](#operator-validation)
- [Adding Custom Operators](#adding-custom-operators)

## Condition Operators

Condition operators are used to compare values in rule conditions. The Rule Engine supports the following condition operators:

| Operator | Description | Alias | Example |
|----------|-------------|-------|---------|
| `equals` | Equality comparison | `==` | `"score": { "operator": "equals", "value": 100 }` |
| `notequals` | Inequality comparison | `!=` | `"status": { "operator": "notequals", "value": "inactive" }` |
| `greaterthan` | Greater than comparison | `>` | `"level": { "operator": "greaterthan", "value": 5 }` |
| `lessthan` | Less than comparison | `<` | `"health": { "operator": "lessthan", "value": 50 }` |
| `greaterthanorequal` | Greater than or equal comparison | `>=` | `"experience": { "operator": ">=", "value": 1000 }` |
| `lessthanorequal` | Less than or equal comparison | `<=` | `"energy": { "operator": "<=", "value": 30 }` |

## Action Operators

Action operators are used to modify values in rule actions. The Rule Engine supports the following action operators:

| Operator | Description | Alias | Example |
|----------|-------------|-------|---------|
| `set` | Sets a value | `=` | `"bonus": { "operator": "set", "value": 1.5 }` |
| `add` | Adds a value | `+=` | `"score": { "operator": "add", "value": 100 }` |
| `subtract` | Subtracts a value | `-=` | `"health": { "operator": "subtract", "value": 10 }` |
| `multiply` | Multiplies by a value | `*=` | `"damage": { "operator": "multiply", "value": 2 }` |
| `divide` | Divides by a value | `/=` | `"cooldown": { "operator": "divide", "value": 2 }` |
| `callback` | Triggers a callback event | `=>` | `"notify": { "operator": "callback", "value": "spin_collected" }` |

Note: For callbacks, both SimpleRules and ExpressionRules support:
- The `=>` operator syntax (e.g., `=> spin_collected`)
- The `callback()` function syntax (e.g., `callback(spin_collected)`)

## Operator Validation

The Rule Engine validates operators to ensure they're supported before executing rules. This helps prevent runtime errors.

Operator validation is performed by the `OperatorRegistry` class, which maintains a list of supported condition and action operators:

```csharp
public class OperatorRegistry
{
    private readonly HashSet<string> _conditionOperators;
    private readonly HashSet<string> _actionOperators;
    
    public OperatorRegistry()
    {
        // Register standard condition operators
        RegisterConditionOperators("equals", "==", "notequals", "!=", 
            "greaterthan", ">", "lessthan", "<", 
            "greaterthanorequal", ">=", "lessthanorequal", "<=");
            
        // Register standard action operators
        RegisterActionOperators("set", "=", "add", "+=", "subtract", "-=",
            "multiply", "*=", "divide", "/=");
    }
    
    // Methods for registering and validating operators
    // ...
}
```

## Adding Custom Operators

You can add custom operators to support your specific needs:

```csharp
// Create a rule engine builder
var builder = new RuleEngineBuilder();

// Register custom condition operators
builder.RegisterConditionOperators("contains", "startsWith", "endsWith");

// Register custom action operators
builder.RegisterActionOperators("append", "prepend");

// Build the rule engine
var ruleEngine = builder.Build();
```

### Implementing Custom Operators

To implement custom operators, you need to create a custom rule factory that handles the custom operators:

```csharp
public class CustomRuleFactory : IRuleFactory
{
    private readonly OperatorRegistry _operatorRegistry;
    
    public CustomRuleFactory(OperatorRegistry operatorRegistry)
    {
        _operatorRegistry = operatorRegistry;
        
        // Register custom operators
        _operatorRegistry.RegisterConditionOperators("contains", "startsWith", "endsWith");
        _operatorRegistry.RegisterActionOperators("append", "prepend");
    }
    
    public string RuleType => "custom";
    
    public IRule Create(RuleDefinition ruleData)
    {
        // Validation and creation logic
        // ...
        
        // Create condition evaluator with custom operators
        Func<IDictionary<string, object>, bool> conditionEvaluator = inputs =>
        {
            // Implement custom operator logic
            // ...
        };
        
        // Create action executor with custom operators
        Func<IDictionary<string, object>, IDictionary<string, object>> actionExecutor = inputs =>
        {
            // Implement custom operator logic
            // ...
        };
        
        return new CustomRule(
            ruleData.RuleId,
            ruleData.RuleName,
            conditionEvaluator,
            actionExecutor
        );
    }
} 