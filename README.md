# Unity Rule Engine

A flexible, framework-agnostic rule engine for Unity-based mobile games that supports JSON-based rule definitions.

## Quick Start

### Prerequisites
- .NET SDK 6.0 or later
- Visual Studio 2022 or VS Code with C# extensions

### Running Locally
1. Clone the repository
2. Open a terminal in the project root directory
3. Run the following commands:
```bash
# Restore dependencies
dotnet restore

# Build the solution
dotnet build

# Run the test program
dotnet run --project RuleEngine.Tests
```

The test program will demonstrate four different rule scenarios:
- Score Bonus Rule
- Level Up Rule
- Multiple Conditions Rule
- NCalc Expression Rule
- Composite Rule

## Features

- JSON-based rule definitions
- Multiple input/output support
- Framework-agnostic design
- Extensible rule system
- Web interface compatible
- Proper type comparison for numeric types
- Nullable reference type support
- NCalc expression support for powerful mathematical and logical expressions
- Composite rules with logical operators (AND, OR, NOT)

## Recent Changes

- Fixed nullable reference warnings
- Improved type comparison logic for numeric values
- Fixed rule evaluation and execution
- Added better error handling
- Updated project to use .NET Standard 2.1 for library and .NET 9.0 for tests
- Added NCalc expression-based rule support
- Added composite rules with logical operators (AND, OR, NOT)

## Project Structure

```
RuleEngine/
├── Core/
│   ├── IRule.cs                 # Rule interface
│   ├── RuleBase.cs             # Base rule implementation
│   ├── RuleEngine.cs           # Main rule engine
│   ├── IRuleParser.cs          # Rule parser interface
│   └── JsonRuleParser.cs       # JSON-based rule parser
├── Rules/
│   ├── SimpleRule.cs           # Basic rule implementation
│   ├── ExpressionRule.cs       # NCalc expression-based rules
│   └── CompositeRule.cs        # Rules with logical operators (AND, OR, NOT)
└── Models/
    └── RuleDefinition.cs       # Rule definition models
```

## Rule Definition Format

### Standard Rules

Rules are defined in JSON format. Here's an example:

```json
{
    "ruleId": "score_bonus",
    "ruleName": "Score Bonus Rule",
    "type": "simple",
    "conditions": {
        "score": {
            "operator": "greaterThan",
            "value": 1000
        }
    },
    "actions": {
        "bonus": {
            "operator": "set",
            "value": 1.5
        }
    }
}
```

### Expression Rules (NCalc)

The rule engine also supports NCalc expressions for more complex calculations:

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

Composite rules allow you to combine multiple rules using logical operators (AND, OR, NOT):

```json
{
    "ruleId": "veteran_premium_player",
    "ruleName": "Veteran Premium Player Rule",
    "type": "composite",
    "operator": "And",
    "rules": [
        {
            "ruleId": "is_premium",
            "ruleName": "Is Premium Rule",
            "type": "simple",
            "conditions": {
                "isPremium": {
                    "operator": "equals",
                    "value": true
                }
            },
            "actions": {}
        },
        {
            "ruleId": "is_veteran",
            "ruleName": "Is Veteran Rule",
            "type": "simple",
            "conditions": {
                "daysPlayed": {
                    "operator": "greaterThanOrEqual",
                    "value": 30
                }
            },
            "actions": {}
        }
    ],
    "actions": {
        "specialReward": {
            "operator": "set",
            "value": "Veteran Premium Chest"
        },
        "gems": {
            "operator": "set",
            "value": 500
        }
    }
}
```

### Rule Structure

#### Simple Rules
- `ruleId`: Unique identifier for the rule
- `ruleName`: Human-readable name
- `type`: Rule type (simple, expression, composite, etc.)
- `conditions`: Input conditions to evaluate
- `actions`: Actions to perform when conditions are met

#### Expression Rules
- `ruleId`: Unique identifier for the rule
- `ruleName`: Human-readable name
- `type`: Must be "expression"
- `conditionExpression`: NCalc expression that evaluates to a boolean
- `actionExpressions`: Dictionary of output names to NCalc expressions

#### Composite Rules
- `ruleId`: Unique identifier for the rule
- `ruleName`: Human-readable name
- `type`: Must be "composite"
- `operator`: Logical operator to combine child rules ("And", "Or", "Not")
- `rules`: Array of child rule definitions
- `actions`: Actions to perform when the composite condition is met (optional)

## Usage Example

### Basic Usage

```csharp
// Create rule engine
var ruleEngine = new RuleEngine(new JsonRuleParser());

// Add rule from JSON
string ruleJson = @"{
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

ruleEngine.AddRule(ruleJson);

// Execute rules
var inputs = new Dictionary<string, object>
{
    { "score", 1500 }
};

var results = ruleEngine.ExecuteRules(inputs);
// results will contain { "bonus": 1.5 }
```

### NCalc Expression Rule

```csharp
string ruleJson = @"{
    ""ruleId"": ""damage_calculation"",
    ""ruleName"": ""Damage Calculation Rule"",
    ""type"": ""expression"",
    ""conditionExpression"": ""attackRoll > defenseRoll"",
    ""actionExpressions"": {
        ""damage"": ""baseDamage * (1 + criticalHit * 0.5) - (defenseValue * 0.2)"",
        ""isCritical"": ""criticalHit == 1"",
        ""effectiveAttack"": ""attackRoll - defenseRoll""
    }
}";

ruleEngine.AddRule(ruleJson);

var inputs = new Dictionary<string, object>
{
    { "attackRoll", 15 },
    { "defenseRoll", 10 },
    { "baseDamage", 20 },
    { "defenseValue", 5 },
    { "criticalHit", 1 }  // 1 = true, 0 = false
};

var results = ruleEngine.ExecuteRules(inputs);
// results may contain calculated damage, isCritical flag, etc.
```

### Multiple Conditions

```csharp
string ruleJson = @"{
    ""ruleId"": ""premium_reward"",
    ""ruleName"": ""Premium Reward Rule"",
    ""type"": ""simple"",
    ""conditions"": {
        ""isPremium"": {
            ""operator"": ""equals"",
            ""value"": true
        },
        ""daysPlayed"": {
            ""operator"": ""greaterThanOrEqual"",
            ""value"": 7
        }
    },
    ""actions"": {
        ""reward"": {
            ""operator"": ""set"",
            ""value"": ""Premium Chest""
        },
        ""coins"": {
            ""operator"": ""set"",
            ""value"": 1500
        }
    }
}";
```

### Multiple Actions

```csharp
string ruleJson = @"{
    ""ruleId"": ""level_up"",
    ""ruleName"": ""Level Up Rule"",
    ""type"": ""simple"",
    ""conditions"": {
        ""experience"": {
            ""operator"": ""greaterThanOrEqual"",
            ""value"": 1000
        }
    },
    ""actions"": {
        ""level"": {
            ""operator"": ""set"",
            ""value"": 2
        },
        ""experience"": {
            ""operator"": ""set"",
            ""value"": 500
        }
    }
}";
```

### Composite Rule

```csharp
string ruleJson = @"{
    ""ruleId"": ""special_offer"",
    ""ruleName"": ""Special Offer Rule"",
    ""type"": ""composite"",
    ""operator"": ""Or"",
    ""rules"": [
        {
            ""ruleId"": ""is_new_player"",
            ""ruleName"": ""Is New Player Rule"",
            ""type"": ""simple"",
            ""conditions"": {
                ""daysPlayed"": {
                    ""operator"": ""lessThan"",
                    ""value"": 7
                }
            },
            ""actions"": {}
        },
        {
            ""ruleId"": ""is_returning_player"",
            ""ruleName"": ""Is Returning Player Rule"",
            ""type"": ""simple"",
            ""conditions"": {
                ""daysSinceLastLogin"": {
                    ""operator"": ""greaterThan"",
                    ""value"": 30
                }
            },
            ""actions"": {}
        }
    ],
    ""actions"": {
        ""specialOffer"": {
            ""operator"": ""set"",
            ""value"": ""Welcome Pack""
        },
        ""discount"": {
            ""operator"": ""set"",
            ""value"": 50
        }
    }
}";

ruleEngine.AddRule(ruleJson);

var inputs = new Dictionary<string, object>
{
    { "daysPlayed", 3 },
    { "daysSinceLastLogin", 0 }
};

var results = ruleEngine.ExecuteRules(inputs);
// If either condition is met (new player OR returning player),
// results will contain a special offer and discount
```

### Combining Composite and Expression Rules

You can combine different rule types for powerful behavior control. Here's an example of a composite rule that uses an expression rule:

```csharp
string ruleJson = @"{
    ""ruleId"": ""advanced_combat_rule"",
    ""ruleName"": ""Advanced Combat Rule"",
    ""type"": ""composite"",
    ""operator"": ""And"",
    ""rules"": [
        {
            ""ruleId"": ""player_can_attack"",
            ""ruleName"": ""Player Can Attack Rule"",
            ""type"": ""simple"",
            ""conditions"": {
                ""isPlayerTurn"": {
                    ""operator"": ""equals"",
                    ""value"": true
                },
                ""playerStamina"": {
                    ""operator"": ""greaterThan"",
                    ""value"": 0
                }
            },
            ""actions"": {}
        },
        {
            ""ruleId"": ""attack_effectiveness"",
            ""ruleName"": ""Attack Effectiveness Rule"",
            ""type"": ""expression"",
            ""conditionExpression"": ""targetIsVulnerable || (weaponType == 'fire' && targetType == 'ice')"",
            ""actionExpressions"": {
                ""damageMultiplier"": ""targetIsVulnerable ? 2.0 : (weaponType == 'fire' && targetType == 'ice' ? 1.5 : 1.0)""
            }
        }
    ],
    ""actions"": {
        ""attackSuccessful"": {
            ""operator"": ""set"",
            ""value"": true
        },
        ""combatMessage"": {
            ""operator"": ""set"",
            ""value"": ""Attack landed successfully!""
        }
    }
}";

ruleEngine.AddRule(ruleJson);

var inputs = new Dictionary<string, object>
{
    { "isPlayerTurn", true },
    { "playerStamina", 10 },
    { "targetIsVulnerable", false },
    { "weaponType", "fire" },
    { "targetType", "ice" }
};

var results = ruleEngine.ExecuteRules(inputs);
// If player can attack AND target is vulnerable or weak to the weapon type,
// the result will contain attackSuccessful=true, damageMultiplier=1.5, and a combat message
```

## NCalc Expression Support

NCalc is a powerful mathematical expressions evaluator. It supports:

### Basic Operators
- Arithmetic: `+`, `-`, `*`, `/`, `%` (modulo)
- Comparison: `>`, `>=`, `<`, `<=`, `==`, `!=`
- Logical: `&&` (and), `||` (or), `!` (not)
- Bitwise: `&`, `|`, `~`, `^`

### Functions
- Math: `Sin`, `Cos`, `Tan`, `Asin`, `Acos`, `Atan`, `Abs`, `Ceiling`, `Floor`, `Exp`, `Log`, `Pow`, `Sqrt`, `Sign`, etc.
- String: `Substring`, `Length`, `Contains`, etc.
- Logical: `if(condition, then, else)`

### Examples
- Simple math: `2 * (3 + 5)`
- Using parameters: `score * multiplier`
- Conditional: `health < 20 ? "Critical" : "Normal"`
- Complex formulas: `damage * (1 - (defense / 100)) * (isCritical ? 2 : 1)`

For more details on NCalc syntax, see the [NCalc documentation](https://github.com/ncalc/ncalc).

## Unity Integration

To use this rule engine in a Unity project:

1. Copy the `RuleEngine` folder to your Unity project's `Assets/Scripts` directory
2. Create a MonoBehaviour that wraps the rule engine functionality:

```csharp
using System.Collections.Generic;
using RuleEngine.Core;
using UnityEngine;

public class GameRuleEngine : MonoBehaviour
{
    private RuleEngine.Core.RuleEngine _ruleEngine;

    private void Awake()
    {
        _ruleEngine = new RuleEngine.Core.RuleEngine(new JsonRuleParser());
        LoadRules();
    }

    private void LoadRules()
    {
        // Load rules from JSON files, PlayerPrefs, or other sources
        TextAsset ruleAsset = Resources.Load<TextAsset>("Rules/score_bonus");
        _ruleEngine.AddRule(ruleAsset.text);
    }

    public Dictionary<string, object> ProcessGameState(Dictionary<string, object> gameState)
    {
        return _ruleEngine.ExecuteRules(gameState);
    }
}
```

## Supported Operators

### Condition Operators
- `equals`
- `notEquals`
- `greaterThan`
- `lessThan`
- `greaterThanOrEqual`
- `lessThanOrEqual`
- `contains`
- `notContains`

### Action Operators
- `set`
- `add`
- `subtract`
- `multiply`
- `divide`
- `append`
- `remove`

## Future Enhancements

- Rule serialization/deserialization to/from binary formats
- Rule validation
- Rule editor UI
- Rule caching for performance optimization
- Rule dependency management

## Contributing

Feel free to submit issues or pull requests with enhancements or bug fixes.

## License

This project is available under the MIT License. 