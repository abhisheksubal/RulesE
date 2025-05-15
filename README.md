# Unity Rule Engine

A flexible, framework-agnostic rule engine for Unity-based mobile games that supports JSON-based rule definitions.

## Features

- JSON-based rule definitions
- Multiple input/output support
- Framework-agnostic design
- Extensible rule system with factory pattern
- Web interface compatible
- Proper type comparison for numeric types
- Nullable reference type support
- NCalc expression support for powerful mathematical and logical expressions
- Composite rules with logical operators (AND, OR, NOT)
- Rule validation
- Builder pattern for easy configuration

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

The test program will demonstrate five different rule scenarios:
- Score Bonus Rule
- Level Up Rule
- Multiple Conditions Rule
- NCalc Expression Rule
- Composite Rule

## Basic Usage

```csharp
// Create rule engine with default configuration
var builder = new RuleEngineBuilder();
var ruleEngine = builder.Build();

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

// Validate the rule
var validator = builder.GetValidator();
List<string> errors;
if (validator.Validate(ruleJson, out errors))
{
    ruleEngine.AddRule(ruleJson);
}

// Execute rules
var inputs = new Dictionary<string, object>
{
    { "score", 1500 }
};

var results = ruleEngine.ExecuteRules(inputs);
// results will contain { "bonus": 1.5 }
```

## Documentation

- [Getting Started](docs/getting-started.md) - Quick start guide
- [Rule Types](docs/rule-types.md) - Detailed information about different rule types
- [Architecture](docs/architecture.md) - Overview of the rule engine architecture
- [Code Examples](docs/examples.md) - Example code for using the rule engine
- [Unity Integration](docs/unity-integration.md) - How to integrate with Unity
- [Extending the Rule Engine](docs/extending.md) - How to add custom rule types
- [NCalc Expressions](docs/ncalc-expressions.md) - Using NCalc expressions in rules
- [Operators](docs/operators.md) - Reference for all supported operators

## Project Structure

```
RuleEngine/
├── Core/
│   ├── IRule.cs                 # Rule interface
│   ├── RuleBase.cs              # Base rule implementation
│   ├── RuleEngine.cs            # Main rule engine
│   ├── IRuleParser.cs           # Rule parser interface
│   ├── JsonRuleParser.cs        # JSON-based rule parser
│   ├── IRuleFactory.cs          # Rule factory interface
│   ├── RuleFactoryRegistry.cs   # Registry for rule factories
│   ├── OperatorRegistry.cs      # Registry for operators
│   ├── RuleValidator.cs         # Rule validator
│   └── RuleEngineBuilder.cs     # Builder for rule engine
├── Rules/
│   ├── SimpleRule.cs            # Basic rule implementation
│   ├── ExpressionRule.cs        # NCalc expression-based rules
│   ├── CompositeRule.cs         # Rules with logical operators (AND, OR, NOT)
│   └── Factories/
│       ├── SimpleRuleFactory.cs     # Factory for simple rules
│       ├── ExpressionRuleFactory.cs # Factory for expression rules
│       └── CompositeRuleFactory.cs  # Factory for composite rules
└── Models/
    └── RuleDefinition.cs        # Rule definition models
```

## Recent Changes

- Added factory pattern for rule creation
- Added operator validation
- Added rule validation
- Added builder pattern for rule engine configuration
- Fixed nullable reference warnings
- Improved type comparison logic for numeric values
- Fixed rule evaluation and execution
- Added better error handling
- Updated project to use .NET Standard 2.1 for library and .NET 9.0 for tests
- Added NCalc expression-based rule support
- Added composite rules with logical operators (AND, OR, NOT)

## Future Enhancements

- Rule serialization/deserialization to/from binary formats
- Rule editor UI
- Rule caching for performance optimization
- Rule dependency management

## Contributing

Feel free to submit issues or pull requests with enhancements or bug fixes.

## License

This project is available under the MIT License. 