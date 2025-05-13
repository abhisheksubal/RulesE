# Extending the Rule Engine

This document explains how to extend the Rule Engine with custom rule types.

## Table of Contents
- [Creating a New Rule Class](#creating-a-new-rule-class)
- [Updating the Rule Definition Model](#updating-the-rule-definition-model)
- [Extending the JsonRuleParser](#extending-the-jsonruleparser)
- [Documenting the New Rule Type](#documenting-the-new-rule-type)
- [Creating Tests](#creating-tests)
- [Example: Time-Based Rule](#example-time-based-rule)

## Creating a New Rule Class

1. Create a new class in the `RuleEngine/Rules` directory that inherits from `RuleBase` and implements the `IRule` interface:

```csharp
using System;
using System.Collections.Generic;
using RuleEngine.Core;

namespace RuleEngine.Rules
{
    public class CustomRule : RuleBase
    {
        // Custom rule-specific fields
        private readonly string _customProperty;
        
        // Constructor
        public CustomRule(
            string ruleId,
            string ruleName,
            string customProperty)
            : base(ruleId, ruleName)
        {
            _customProperty = customProperty ?? throw new ArgumentNullException(nameof(customProperty));
        }
        
        // Implement the Evaluate method
        public override bool Evaluate(IDictionary<string, object> inputs)
        {
            try
            {
                // Custom evaluation logic
                return true; // Replace with actual evaluation logic
            }
            catch (Exception)
            {
                return false;
            }
        }
        
        // Implement the Execute method
        public override IDictionary<string, object> Execute(IDictionary<string, object> inputs)
        {
            try
            {
                // Custom execution logic
                var results = new Dictionary<string, object>();
                // Populate results based on your custom rule's logic
                return results;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error executing rule {RuleId}: {ex.Message}", ex);
            }
        }
    }
}
```

## Updating the Rule Definition Model

1. Add new properties to the `RuleDefinition` class in `RuleEngine/Models/RuleDefinition.cs` to support your custom rule type:

```csharp
// For custom rule
[JsonProperty("customProperty")]
public string? CustomProperty { get; set; }
```

## Extending the JsonRuleParser

1. Modify the `JsonRuleParser.cs` file to handle the new rule type:
   - Add a new case to the switch statement in the `Parse` method:
   
```csharp
switch (ruleData.Type?.ToLower())
{
    case "simple":
        return CreateSimpleRule(ruleData);
    case "expression":
        return CreateExpressionRule(ruleData);
    case "composite":
        return CreateCompositeRule(ruleData);
    case "custom": // Your custom rule type
        return CreateCustomRule(ruleData);
    default:
        throw new ArgumentException($"Unsupported rule type: {ruleData.Type}");
}
```

2. Create a method to instantiate your custom rule:

```csharp
private IRule CreateCustomRule(RuleDefinition ruleData)
{
    if (string.IsNullOrEmpty(ruleData.CustomProperty))
    {
        throw new ArgumentException("CustomProperty is required for CustomRule");
    }

    return new CustomRule(
        ruleData.RuleId!,
        ruleData.RuleName!,
        ruleData.CustomProperty
    );
}
```

## Documenting the New Rule Type

1. Add documentation for your custom rule type, including:
   - JSON schema
   - Example rule definition
   - Explanation of properties and behavior

```json
{
    "ruleId": "custom_rule_example",
    "ruleName": "Custom Rule Example",
    "type": "custom",
    "customProperty": "exampleValue"
}
```

## Creating Tests

1. Create unit tests for your custom rule type in the `RuleEngine.Tests` project to ensure it works correctly:
   - Test rule parsing
   - Test rule evaluation
   - Test rule execution
   - Test integration with the rule engine

## Example: Time-Based Rule

Here's an example of a time-based rule that evaluates based on the current time:

```csharp
public class TimeBasedRule : RuleBase
{
    private readonly TimeSpan _startTime;
    private readonly TimeSpan _endTime;
    private readonly Dictionary<string, object> _actions;

    public TimeBasedRule(
        string ruleId,
        string ruleName,
        TimeSpan startTime,
        TimeSpan endTime,
        Dictionary<string, object> actions)
        : base(ruleId, ruleName)
    {
        _startTime = startTime;
        _endTime = endTime;
        _actions = actions ?? throw new ArgumentNullException(nameof(actions));
    }

    public override bool Evaluate(IDictionary<string, object> inputs)
    {
        try
        {
            var currentTime = DateTime.Now.TimeOfDay;
            return currentTime >= _startTime && currentTime <= _endTime;
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
            // Return the predefined actions
            return _actions.ToDictionary(a => a.Key, a => a.Value);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error executing rule {RuleId}: {ex.Message}", ex);
        }
    }
}
```

### JSON Definition for Time-Based Rule

```json
{
    "ruleId": "happy_hour",
    "ruleName": "Happy Hour Discount",
    "type": "timeBasedRule",
    "startTime": "17:00:00",
    "endTime": "19:00:00",
    "actions": {
        "discount": {
            "operator": "set",
            "value": 0.2
        },
        "message": {
            "operator": "set",
            "value": "Happy Hour - 20% off all items!"
        }
    }
}
``` 