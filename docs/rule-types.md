# Rule Types

This document describes the different types of rules supported by the Unity Rule Engine.

## Table of Contents
- [Simple Rules](#simple-rules)
- [Expression Rules](#expression-rules)
- [Composite Rules](#composite-rules)

## Simple Rules

Simple rules are the most basic type of rules in the engine. They consist of conditions and actions.

### Rule Structure
- `ruleId`: Unique identifier for the rule
- `ruleName`: Human-readable name
- `type`: Must be "simple"
- `conditions`: Input conditions to evaluate
- `actions`: Actions to perform when conditions are met

### JSON Format

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

### Multiple Conditions

Simple rules can have multiple conditions, all of which must be met for the rule to trigger:

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

### Multiple Actions

Simple rules can also have multiple actions that are executed when the conditions are met:

```json
{
    "ruleId": "level_up",
    "ruleName": "Level Up Rule",
    "type": "simple",
    "conditions": {
        "experience": {
            "operator": "greaterThanOrEqual",
            "value": 1000
        }
    },
    "actions": {
        "level": {
            "operator": "set",
            "value": 2
        },
        "experience": {
            "operator": "set",
            "value": 500
        }
    }
}
```

## Expression Rules

Expression rules use NCalc expressions for more complex calculations and conditions.

### Rule Structure
- `ruleId`: Unique identifier for the rule
- `ruleName`: Human-readable name
- `type`: Must be "expression"
- `conditionExpression`: NCalc expression that evaluates to a boolean
- `actionExpressions`: Dictionary of output names to NCalc expressions

### JSON Format

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

## Composite Rules

Composite rules allow you to combine multiple rules using logical operators (AND, OR, NOT).

### Rule Structure
- `ruleId`: Unique identifier for the rule
- `ruleName`: Human-readable name
- `type`: Must be "composite"
- `operator`: Logical operator to combine child rules ("And", "Or", "Not")
- `rules`: Array of child rule definitions
- `actions`: Actions to perform when the composite condition is met (optional)

### JSON Format

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

### OR Operator Example

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

### NOT Operator Example

```json
{
    "ruleId": "not_tutorial_completed",
    "ruleName": "Tutorial Not Completed Rule",
    "type": "composite",
    "operator": "Not",
    "rules": [
        {
            "ruleId": "tutorial_completed",
            "ruleName": "Tutorial Completed Rule",
            "type": "simple",
            "conditions": {
                "tutorialCompleted": {
                    "operator": "equals",
                    "value": true
                }
            },
            "actions": {}
        }
    ],
    "actions": {
        "showTutorial": {
            "operator": "set",
            "value": true
        }
    }
}
``` 