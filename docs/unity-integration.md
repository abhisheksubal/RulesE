# Unity Integration

This document explains how to integrate the Rule Engine into a Unity project using the DLL-based approach.

## Table of Contents
- [Setup](#setup)
- [Creating a MonoBehaviour Wrapper](#creating-a-monobehaviour-wrapper)
- [Storing Rule Definitions](#storing-rule-definitions)
- [Validating Rules](#validating-rules)
- [Testing in Unity](#testing-in-unity)
- [Example Integration](#example-integration)

## Setup

To use this rule engine in a Unity project:

1. Build the RuleEngine project:
   - Open the solution in Visual Studio or your preferred IDE
   - Build the project in Release mode
   - The DLLs will be generated in the `RuleEngine/bin/Release/netstandard2.1` directory

2. In your Unity project:
   - Create a `Plugins` folder in your Unity project's Assets directory if it doesn't exist
   - Copy the following files from the RuleEngine build output to the Unity Plugins folder:
     - `RuleEngine.dll`
     - `NCalc.dll`
     - `Newtonsoft.Json.dll`

3. For IL2CPP builds, create a `link.xml` file in your Assets folder:
```xml
<linker>
  <assembly fullname="NCalc" preserve="all"/>
  <assembly fullname="RuleEngine" preserve="all"/>
  <assembly fullname="Newtonsoft.Json" preserve="all"/>
</linker>
```

## Creating a MonoBehaviour Wrapper

Create a MonoBehaviour wrapper to use the rule engine in Unity:

```csharp
using System.Collections.Generic;
using RuleEngine.Core;
using UnityEngine;

public class GameRuleEngine : MonoBehaviour
{
    private RuleEngine.Core.RuleEngine _ruleEngine;
    private RuleValidator _ruleValidator;

    private void Awake()
    {
        // Create a rule engine builder with default configuration
        var builder = new RuleEngineBuilder();
        
        // Get the validator for rule validation
        _ruleValidator = builder.GetValidator();
        
        // Build the rule engine
        _ruleEngine = builder.Build();
        
        // Load rules
        LoadRules();
    }

    private void LoadRules()
    {
        // Load rules from JSON files, PlayerPrefs, or other sources
        TextAsset ruleAsset = Resources.Load<TextAsset>("Rules/score_bonus");
        
        // Validate the rule before adding it
        List<string> errors;
        if (_ruleValidator.Validate(ruleAsset.text, out errors))
        {
            _ruleEngine.AddRule(ruleAsset.text);
            Debug.Log($"Loaded rule: {ruleAsset.name}");
        }
        else
        {
            Debug.LogError($"Invalid rule {ruleAsset.name}: {string.Join(", ", errors)}");
        }
    }

    public Dictionary<string, object> ProcessGameState(Dictionary<string, object> gameState)
    {
        return _ruleEngine.ExecuteRules(gameState);
    }
}
```

## Storing Rule Definitions

1. Create a `Resources/Rules` folder in your Unity project
2. Save your rule JSON definitions as text files in this folder
3. Access them using `Resources.Load<TextAsset>("Rules/your_rule_name")`

## Validating Rules

It's important to validate rules before adding them to the engine, especially in a game environment where invalid rules could cause runtime errors:

```csharp
private void ValidateAndAddRule(string ruleJson)
{
    List<string> errors;
    if (_ruleValidator.Validate(ruleJson, out errors))
    {
        _ruleEngine.AddRule(ruleJson);
        Debug.Log("Rule added successfully");
    }
    else
    {
        Debug.LogError($"Invalid rule: {string.Join(", ", errors)}");
    }
}
```

## Testing in Unity

1. Create a simple test script that loads and executes rules
2. Use Unity's Debug.Log to verify rule outputs
3. Test on both the Editor and target platforms

## Example Integration

Here's an example of using the rule engine in a game context:

```csharp
public class PlayerController : MonoBehaviour
{
    [SerializeField] private GameRuleEngine ruleEngine;
    
    private void ApplyGameRules()
    {
        // Collect current game state
        var gameState = new Dictionary<string, object>
        {
            { "playerScore", 1500 },
            { "isPremium", PlayerPrefs.GetInt("premium", 0) == 1 },
            { "daysPlayed", PlayerPrefs.GetInt("days_played", 0) },
            { "playerLevel", PlayerStats.CurrentLevel }
        };
        
        // Process rules
        var results = ruleEngine.ProcessGameState(gameState);
        
        // Apply results
        if (results.TryGetValue("bonus", out var bonus))
        {
            ApplyScoreBonus(Convert.ToDouble(bonus));
        }
        
        if (results.TryGetValue("specialOffer", out var offer))
        {
            ShowSpecialOffer(offer.ToString());
        }
    }
}
```

### Advanced Configuration

You can customize the rule engine for your game's specific needs:

```csharp
// Create a rule engine with custom configuration
var builder = new RuleEngineBuilder()
    // Register game-specific condition operators
    .RegisterConditionOperators("hasItem", "hasQuest", "hasAchievement")
    
    // Register game-specific action operators
    .RegisterActionOperators("giveItem", "removeItem", "triggerEvent")
    
    // Register a custom rule factory for game-specific rules
    .RegisterFactory(new QuestRuleFactory());

// Build the rule engine
_ruleEngine = builder.Build();
```

## Updating the Rule Engine

When you need to update the Rule Engine:

1. Rebuild the RuleEngine project
2. Copy the new DLLs from the build output to your Unity project's Plugins folder
3. Unity will automatically recompile and update the references 