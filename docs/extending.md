# Extending the Rule Engine

This document explains how to extend the Rule Engine with custom rule types.

## Table of Contents
- [Creating a New Rule Class](#creating-a-new-rule-class)
- [Creating a Rule Factory](#creating-a-rule-factory)
- [Registering the Factory](#registering-the-factory)
- [Updating the Rule Definition Model](#updating-the-rule-definition-model)
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

## Creating a Rule Factory

1. Create a factory class in the `RuleEngine/Rules/Factories` directory that implements the `IRuleFactory` interface:

```csharp
using System;
using RuleEngine.Core;
using RuleEngine.Models;

namespace RuleEngine.Rules.Factories
{
    public class CustomRuleFactory : IRuleFactory
    {
        // Define the rule type this factory handles
        public string RuleType => "custom";
        
        // Create a rule instance from rule definition data
        public IRule Create(RuleDefinition ruleData)
        {
            if (ruleData == null)
                throw new ArgumentNullException(nameof(ruleData));
                
            if (string.IsNullOrEmpty(ruleData.RuleId))
                throw new ArgumentException("RuleId is required for CustomRule");
                
            if (string.IsNullOrEmpty(ruleData.RuleName))
                throw new ArgumentException("RuleName is required for CustomRule");
                
            if (string.IsNullOrEmpty(ruleData.CustomProperty))
                throw new ArgumentException("CustomProperty is required for CustomRule");
                
            return new CustomRule(
                ruleData.RuleId,
                ruleData.RuleName,
                ruleData.CustomProperty
            );
        }
    }
}
```

## Registering the Factory

1. Register your factory with the `RuleFactoryRegistry` using the `RuleEngineBuilder`:

```csharp
// Create a rule engine builder
var builder = new RuleEngineBuilder();

// Register your custom rule factory
builder.RegisterFactory(new CustomRuleFactory());

// Build the rule engine
var ruleEngine = builder.Build();
```

## Updating the Rule Definition Model

1. Add new properties to the `RuleDefinition` class in `RuleEngine/Models/RuleDefinition.cs` to support your custom rule type:

```csharp
// For custom rule
[JsonProperty("customProperty")]
public string? CustomProperty { get; set; }
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

### Rule Class

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

### Rule Factory

```csharp
public class TimeBasedRuleFactory : IRuleFactory
{
    public string RuleType => "timeBasedRule";
    
    public IRule Create(RuleDefinition ruleData)
    {
        if (ruleData == null)
            throw new ArgumentNullException(nameof(ruleData));
            
        if (string.IsNullOrEmpty(ruleData.RuleId))
            throw new ArgumentException("RuleId is required for TimeBasedRule");
            
        if (string.IsNullOrEmpty(ruleData.RuleName))
            throw new ArgumentException("RuleName is required for TimeBasedRule");
            
        // Parse start and end times
        if (!TimeSpan.TryParse(ruleData.StartTime, out var startTime))
            throw new ArgumentException("Invalid StartTime format");
            
        if (!TimeSpan.TryParse(ruleData.EndTime, out var endTime))
            throw new ArgumentException("Invalid EndTime format");
            
        // Extract actions
        var actions = new Dictionary<string, object>();
        if (ruleData.Actions != null)
        {
            foreach (var action in ruleData.Actions)
            {
                actions[action.Key] = action.Value.Value;
            }
        }
        
        return new TimeBasedRule(
            ruleData.RuleId,
            ruleData.RuleName,
            startTime,
            endTime,
            actions
        );
    }
}
```

### Model Updates

```csharp
// For time-based rule
[JsonProperty("startTime")]
public string? StartTime { get; set; }

[JsonProperty("endTime")]
public string? EndTime { get; set; }
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