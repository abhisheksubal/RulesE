# Rule Engine Architecture

This document explains the architecture of the rule engine, including the factory pattern for rule creation and the validation approach.

## Table of Contents
- [Overview](#overview)
- [Factory Pattern](#factory-pattern)
- [Rule Validation](#rule-validation)
- [Builder Pattern](#builder-pattern)
- [Class Diagram](#class-diagram)

## Overview

The rule engine is designed with extensibility and maintainability in mind. It uses several design patterns to achieve these goals:

1. **Factory Pattern**: Each rule type has a dedicated factory for creating rule instances
2. **Registry Pattern**: Factories and operators are registered in central registries
3. **Builder Pattern**: A builder class simplifies the setup and configuration of the rule engine
4. **Validation**: Rules are validated before they're added to the engine

## Factory Pattern

The factory pattern is used to create rule instances from rule definitions. This approach makes it easy to add new rule types without modifying existing code.

### Core Components

- `IRuleFactory`: Interface for rule factories
- `RuleFactoryRegistry`: Registry for rule factories
- Concrete factories:
  - `SimpleRuleFactory`: Creates simple rules
  - `ExpressionRuleFactory`: Creates expression rules
  - `CompositeRuleFactory`: Creates composite rules

### Adding a New Rule Type

To add a new rule type:

1. Create a new rule class implementing `IRule`
2. Create a factory for the rule type implementing `IRuleFactory`
3. Register the factory with the `RuleFactoryRegistry`

Example:

```csharp
// 1. Create a new rule class
public class TimedRule : RuleBase
{
    // Implementation...
}

// 2. Create a factory for the rule type
public class TimedRuleFactory : IRuleFactory
{
    public string RuleType => "timed";
    
    public IRule Create(RuleDefinition ruleData)
    {
        // Validation and creation logic...
        return new TimedRule(...);
    }
}

// 3. Register the factory
var builder = new RuleEngineBuilder();
builder.RegisterFactory(new TimedRuleFactory());
```

## Rule Validation

Rules are validated to ensure they're well-formed before they're added to the engine. This helps prevent runtime errors.

### Validation Components

- `OperatorRegistry`: Defines and validates supported operators
- `RuleValidator`: Validates rule definitions

### Operator Validation

The `OperatorRegistry` provides a centralized place to define and validate operators:

```csharp
// Register custom operators
var builder = new RuleEngineBuilder();
builder.RegisterConditionOperators("contains", "startsWith", "endsWith");
builder.RegisterActionOperators("append", "prepend");
```

### Rule Validation

The `RuleValidator` ensures rules are valid:

```csharp
// Validate a rule
var validator = builder.GetValidator();
List<string> errors;
if (!validator.Validate(ruleJson, out errors))
{
    // Handle validation errors
    Console.WriteLine(string.Join(", ", errors));
}
```

## Builder Pattern

The `RuleEngineBuilder` simplifies the setup and configuration of the rule engine:

```csharp
// Create a rule engine with default configuration
var builder = new RuleEngineBuilder();
var ruleEngine = builder.Build();

// Create a rule engine with custom configuration
var builder = new RuleEngineBuilder()
    .RegisterConditionOperators("contains", "startsWith", "endsWith")
    .RegisterActionOperators("append", "prepend")
    .RegisterFactory(new CustomRuleFactory());
var ruleEngine = builder.Build();
```

## Class Diagram

```
+----------------+      +------------------+      +----------------+
| RuleEngine     |----->| JsonRuleParser   |----->| RuleFactoryRegistry |
+----------------+      +------------------+      +----------------+
                                                 /|\
                                                  |
                         +---------------------+  |  +----------------------+
                         | SimpleRuleFactory   |--+--| ExpressionRuleFactory |
                         +---------------------+  |  +----------------------+
                                                  |
                                                  |  +---------------------+
                                                  +--| CompositeRuleFactory |
                                                     +---------------------+

+----------------+      +------------------+
| RuleValidator  |----->| OperatorRegistry |
+----------------+      +------------------+

+----------------+
| RuleEngineBuilder |
+----------------+
       |
       |
       v
+----------------+      +------------------+      +----------------+
| RuleEngine     |----->| JsonRuleParser   |----->| RuleFactoryRegistry |
+----------------+      +------------------+      +----------------+
``` 