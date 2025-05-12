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

The test program will demonstrate three different rule scenarios:
- Score Bonus Rule
- Level Up Rule
- Multiple Conditions Rule

## Features

- JSON-based rule definitions
- Multiple input/output support
- Framework-agnostic design
- Extensible rule system
- Web interface compatible

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
│   └── CompositeRule.cs        # Complex rule implementation
└── Models/
    └── RuleDefinition.cs       # Rule definition models
```

## Rule Definition Format

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
            "operator": "multiply",
            "value": 1.5
        }
    }
}
```

### Rule Structure

- `ruleId`: Unique identifier for the rule
- `ruleName`: Human-readable name
- `type`: Rule type (simple, composite, etc.)
- `conditions`: Input conditions to evaluate
- `actions`: Actions to perform when conditions are met

## Usage Example

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
            ""operator"": ""multiply"",
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