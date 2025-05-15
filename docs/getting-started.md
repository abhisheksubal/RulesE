# Getting Started with the Rule Engine

This document provides a quick start guide for using the Rule Engine.

## Table of Contents
- [Installation](#installation)
- [Basic Usage](#basic-usage)
- [Creating Rules](#creating-rules)
- [Executing Rules](#executing-rules)
- [Validating Rules](#validating-rules)
- [Advanced Configuration](#advanced-configuration)

## Installation

1. Add the Rule Engine to your project:

```bash
# Using NuGet Package Manager
Install-Package RuleEngine

# Using .NET CLI
dotnet add package RuleEngine
```

2. Import the necessary namespaces:

```csharp
using RuleEngine.Core;
using RuleEngine.Models;
```

## Basic Usage

The simplest way to use the Rule Engine is with the `RuleEngineBuilder`:

```csharp
// Create a rule engine with default configuration
var builder = new RuleEngineBuilder();
var ruleEngine = builder.Build();

// Add rules
ruleEngine.AddRule(@"
{
    ""ruleId"": ""score_bonus"",
    ""ruleName"": ""Score Bonus Rule"",
    ""type"": ""simple"",
    ""conditions"": {
        ""score"": {
            ""operator"": ""greaterThan"",
            ""value"": 1000
        }
    },
    ""actions"": {
        ""bonus"": {
            ""operator"": ""set"",
            ""value"": 1.5
        }
    }
}");

// Execute rules
var inputs = new Dictionary<string, object>
{
    { "score", 1500 }
};

var results = ruleEngine.ExecuteRules(inputs);

// Access results
var bonus = results["bonus"]; // 1.5
```

## Creating Rules

Rules are defined in JSON format. The Rule Engine supports three types of rules:

### Simple Rules

```json
{
    "ruleId": "premium_reward",
    "ruleName": "Premium Reward Rule",
    "type": "simple",
    "conditions": {
        "isPremium": {
            "operator": "equals",
            "value": true
        },
        "daysPlayed": {
            "operator": "greaterThanOrEqual",
            "value": 7
        }
    },
    "actions": {
        "reward": {
            "operator": "set",
            "value": "Premium Chest"
        },
        "coins": {
            "operator": "set",
            "value": 1500
        }
    }
}
```

### Expression Rules

```json
{
    "ruleId": "damage_calculation",
    "ruleName": "Damage Calculation Rule",
    "type": "expression",
    "conditionExpression": "attackRoll > defenseRoll",
    "actionExpressions": {
        "damage": "baseDamage * (1 + criticalHit * 0.5) - (defenseValue * 0.2)",
        "isCritical": "criticalHit == 1",
        "effectiveAttack": "attackRoll - defenseRoll"
    }
}
```

### Composite Rules

```json
{
    "ruleId": "special_offer",
    "ruleName": "Special Offer Rule",
    "type": "composite",
    "operator": "Or",
    "rules": [
        {
            "ruleId": "is_new_player",
            "ruleName": "Is New Player Rule",
            "type": "simple",
            "conditions": {
                "daysPlayed": {
                    "operator": "lessThan",
                    "value": 7
                }
            },
            "actions": {}
        },
        {
            "ruleId": "is_returning_player",
            "ruleName": "Is Returning Player Rule",
            "type": "simple",
            "conditions": {
                "daysSinceLastLogin": {
                    "operator": "greaterThan",
                    "value": 30
                }
            },
            "actions": {}
        }
    ],
    "actions": {
        "specialOffer": {
            "operator": "set",
            "value": "Welcome Pack"
        },
        "discount": {
            "operator": "set",
            "value": 50
        }
    }
}
```

## Executing Rules

To execute rules against a set of inputs:

```csharp
// Create inputs
var inputs = new Dictionary<string, object>
{
    { "score", 1500 },
    { "isPremium", true },
    { "daysPlayed", 10 }
};

// Execute rules
var results = ruleEngine.ExecuteRules(inputs);

// Access results
foreach (var result in results)
{
    Console.WriteLine($"{result.Key}: {result.Value}");
}
```

## Validating Rules

You can validate rules before adding them to the engine:

```csharp
// Get the validator from the builder
var validator = builder.GetValidator();

// Validate a rule
string ruleJson = @"
{
    ""ruleId"": ""score_bonus"",
    ""ruleName"": ""Score Bonus Rule"",
    ""type"": ""simple"",
    ""conditions"": {
        ""score"": {
            ""operator"": ""greaterThan"",
            ""value"": 1000
        }
    },
    ""actions"": {
        ""bonus"": {
            ""operator"": ""set"",
            ""value"": 1.5
        }
    }
}";

List<string> errors;
if (validator.Validate(ruleJson, out errors))
{
    // Rule is valid, add it to the engine
    ruleEngine.AddRule(ruleJson);
}
else
{
    // Handle validation errors
    Console.WriteLine(string.Join(", ", errors));
}
```

## Advanced Configuration

The `RuleEngineBuilder` allows you to customize the rule engine:

```csharp
// Create a rule engine with custom configuration
var builder = new RuleEngineBuilder()
    // Register custom condition operators
    .RegisterConditionOperators("contains", "startsWith", "endsWith")
    
    // Register custom action operators
    .RegisterActionOperators("append", "prepend")
    
    // Register a custom rule factory
    .RegisterFactory(new CustomRuleFactory());

// Build the rule engine
var ruleEngine = builder.Build();
``` 