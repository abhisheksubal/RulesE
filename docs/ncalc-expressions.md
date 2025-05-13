# NCalc Expression Support

This document explains how to use NCalc expressions in the Rule Engine.

## Table of Contents
- [Basic Operators](#basic-operators)
- [Functions](#functions)
- [Examples](#examples)

## Basic Operators

NCalc is a powerful mathematical expressions evaluator. It supports:

### Arithmetic Operators
- Addition: `+`
- Subtraction: `-`
- Multiplication: `*`
- Division: `/`
- Modulo: `%`

### Comparison Operators
- Greater than: `>`
- Greater than or equal: `>=`
- Less than: `<`
- Less than or equal: `<=`
- Equal: `==`
- Not equal: `!=`

### Logical Operators
- And: `&&`
- Or: `||`
- Not: `!`

### Bitwise Operators
- And: `&`
- Or: `|`
- Not: `~`
- Xor: `^`

## Functions

### Math Functions
- `Sin`, `Cos`, `Tan`, `Asin`, `Acos`, `Atan`
- `Abs`, `Ceiling`, `Floor`
- `Exp`, `Log`, `Log10`
- `Pow`, `Sqrt`, `Sign`
- `Max`, `Min`
- `Round`

### String Functions
- `Substring`
- `Length`
- `Contains`
- `StartsWith`
- `EndsWith`
- `ToLower`
- `ToUpper`

### Logical Functions
- `if(condition, then, else)`

## Examples

### Simple Math
```
2 * (3 + 5)  // Result: 16
```

### Using Parameters
```
score * multiplier  // Uses values from inputs
```

### Conditional Expressions
```
health < 20 ? "Critical" : "Normal"
```

### Complex Formulas
```
damage * (1 - (defense / 100)) * (isCritical ? 2 : 1)
```

### Using in Rule Definitions

```json
{
    "ruleId": "complex_damage",
    "ruleName": "Complex Damage Calculation",
    "type": "expression",
    "conditionExpression": "attackRoll > defenseRoll && playerHasWeapon",
    "actionExpressions": {
        "damage": "baseDamage * (1 + (criticalChance / 100)) * if(targetIsArmored, 0.5, 1.0)",
        "hitLocation": "targetHeight > 1.7 ? 'head' : 'body'",
        "effectiveAttack": "attackRoll - defenseRoll + attackBonus"
    }
}
```

For more details on NCalc syntax, see the [NCalc documentation](https://github.com/ncalc/ncalc). 