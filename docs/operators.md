# Supported Operators

This document lists all operators supported by the Rule Engine.

## Table of Contents
- [Condition Operators](#condition-operators)
- [Action Operators](#action-operators)

## Condition Operators

These operators are used in rule conditions to evaluate inputs:

| Operator | Description | Example |
|----------|-------------|---------|
| `equals` | Checks if two values are equal | `"score": { "operator": "equals", "value": 100 }` |
| `notEquals` | Checks if two values are not equal | `"status": { "operator": "notEquals", "value": "inactive" }` |
| `greaterThan` | Checks if a value is greater than another | `"score": { "operator": "greaterThan", "value": 1000 }` |
| `lessThan` | Checks if a value is less than another | `"health": { "operator": "lessThan", "value": 50 }` |
| `greaterThanOrEqual` | Checks if a value is greater than or equal to another | `"level": { "operator": "greaterThanOrEqual", "value": 10 }` |
| `lessThanOrEqual` | Checks if a value is less than or equal to another | `"energy": { "operator": "lessThanOrEqual", "value": 20 }` |
| `contains` | Checks if a string contains another string or an array contains a value | `"tags": { "operator": "contains", "value": "premium" }` |
| `notContains` | Checks if a string does not contain another string or an array does not contain a value | `"inventory": { "operator": "notContains", "value": "key" }` |

## Action Operators

These operators are used in rule actions to modify outputs:

| Operator | Description | Example |
|----------|-------------|---------|
| `set` | Sets a value | `"bonus": { "operator": "set", "value": 1.5 }` |
| `add` | Adds a value to an existing numeric value | `"score": { "operator": "add", "value": 100 }` |
| `subtract` | Subtracts a value from an existing numeric value | `"health": { "operator": "subtract", "value": 10 }` |
| `multiply` | Multiplies an existing numeric value by another value | `"damage": { "operator": "multiply", "value": 1.5 }` |
| `divide` | Divides an existing numeric value by another value | `"cooldown": { "operator": "divide", "value": 2 }` |
| `append` | Appends a value to an existing string or array | `"message": { "operator": "append", "value": " bonus applied" }` |
| `remove` | Removes a value from an existing string or array | `"inventory": { "operator": "remove", "value": "used_potion" }` |

## Usage Examples

### Condition Operators Example

```json
{
    "ruleId": "veteran_player_bonus",
    "ruleName": "Veteran Player Bonus",
    "type": "simple",
    "conditions": {
        "daysPlayed": {
            "operator": "greaterThanOrEqual",
            "value": 30
        },
        "accountStatus": {
            "operator": "equals",
            "value": "active"
        },
        "badges": {
            "operator": "contains",
            "value": "loyal_player"
        }
    },
    "actions": {
        "dailyReward": {
            "operator": "multiply",
            "value": 2
        },
        "message": {
            "operator": "set",
            "value": "Thank you for being a loyal player!"
        }
    }
}
```

### Action Operators Example

```json
{
    "ruleId": "level_up_rewards",
    "ruleName": "Level Up Rewards",
    "type": "simple",
    "conditions": {
        "leveledUp": {
            "operator": "equals",
            "value": true
        }
    },
    "actions": {
        "coins": {
            "operator": "add",
            "value": 500
        },
        "energy": {
            "operator": "set",
            "value": 100
        },
        "cooldowns": {
            "operator": "divide",
            "value": 2
        },
        "playerTitle": {
            "operator": "append",
            "value": " the Experienced"
        }
    }
}
``` 