# Code Examples

This document provides code examples for using the Unity Rule Engine.

## Table of Contents
- [Basic Usage](#basic-usage)
- [NCalc Expression Rule](#ncalc-expression-rule)
- [Composite Rule](#composite-rule)
- [Combining Rule Types](#combining-rule-types)

## Basic Usage

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

## NCalc Expression Rule

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

## Composite Rule

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

## Combining Rule Types

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