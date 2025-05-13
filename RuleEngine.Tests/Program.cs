using System;
using System.Collections.Generic;
using RuleEngine.Core;
using RuleEngine.Models;

namespace RuleEngine.Tests
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Rule Engine Test Program");
            Console.WriteLine("=======================");

            // Create rule engine
            var ruleEngine = new Core.RuleEngine(new JsonRuleParser());

            // Test 1: Simple Score Bonus Rule
            TestScoreBonusRule(ruleEngine);

            // Test 2: Level Up Rule
            TestLevelUpRule(ruleEngine);

            // Test 3: Multiple Conditions Rule
            TestMultipleConditionsRule(ruleEngine);
            
            // Test 4: NCalc Expression Rule
            TestExpressionRule(ruleEngine);
            
            // Test 5: Composite Rule
            TestCompositeRule(ruleEngine);

            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }

        static void TestScoreBonusRule(Core.RuleEngine ruleEngine)
        {
            Console.WriteLine("\nTest 1: Score Bonus Rule");
            Console.WriteLine("------------------------");
            Console.WriteLine("Description: A simple rule that applies a 1.5x bonus when score exceeds 1000");
            Console.WriteLine("Rule Type: Simple");

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

            Console.WriteLine("Rule Definition:");
            Console.WriteLine("- Condition: score > 1000");
            Console.WriteLine("- Action: Set bonus = 1.5");
            Console.WriteLine("\nTest Cases:");

            ruleEngine.AddRule(ruleJson);

            // Test case 1: Score above threshold
            var inputs1 = new Dictionary<string, object>
            {
                { "score", 1500 }
            };

            var results1 = ruleEngine.ExecuteRules(inputs1);
            Console.WriteLine("1. Score = 1500 (above threshold)");
            Console.WriteLine($"   Result: Bonus Applied = {(results1.ContainsKey("bonus") ? results1["bonus"] : "No bonus")}");

            // Test case 2: Score below threshold
            var inputs2 = new Dictionary<string, object>
            {
                { "score", 500 }
            };

            var results2 = ruleEngine.ExecuteRules(inputs2);
            Console.WriteLine("2. Score = 500 (below threshold)");
            Console.WriteLine($"   Result: Bonus Applied = {(results2.ContainsKey("bonus") ? results2["bonus"] : "No bonus")}");
        }

        static void TestLevelUpRule(Core.RuleEngine ruleEngine)
        {
            Console.WriteLine("\nTest 2: Level Up Rule");
            Console.WriteLine("--------------------");
            Console.WriteLine("Description: A rule that increases player level and adjusts experience when reaching 1000+ XP");
            Console.WriteLine("Rule Type: Simple with multiple actions");

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

            Console.WriteLine("Rule Definition:");
            Console.WriteLine("- Condition: experience >= 1000");
            Console.WriteLine("- Action 1: Set level = 2");
            Console.WriteLine("- Action 2: Set experience = 500 (remaining after level up)");
            Console.WriteLine("\nTest Case:");

            ruleEngine.AddRule(ruleJson);

            var inputs = new Dictionary<string, object>
            {
                { "experience", 1500 },
                { "level", 1 }
            };

            var results = ruleEngine.ExecuteRules(inputs);
            Console.WriteLine("Initial State: Level = 1, Experience = 1500");
            Console.WriteLine($"Result: New Level = {(results.ContainsKey("level") ? results["level"] : inputs["level"])}");
            Console.WriteLine($"        Remaining Experience = {(results.ContainsKey("experience") ? results["experience"] : inputs["experience"])}");
        }

        static void TestMultipleConditionsRule(Core.RuleEngine ruleEngine)
        {
            Console.WriteLine("\nTest 3: Multiple Conditions Rule");
            Console.WriteLine("--------------------------------");
            Console.WriteLine("Description: A rule that requires multiple conditions to be met (AND logic)");
            Console.WriteLine("Rule Type: Simple with multiple conditions");

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

            Console.WriteLine("Rule Definition:");
            Console.WriteLine("- Condition 1: isPremium = true");
            Console.WriteLine("- Condition 2: daysPlayed >= 7");
            Console.WriteLine("- Action 1: Set reward = \"Premium Chest\"");
            Console.WriteLine("- Action 2: Set coins = 1500");
            Console.WriteLine("\nTest Cases:");

            ruleEngine.AddRule(ruleJson);

            // Test case 1: Eligible for reward
            var inputs1 = new Dictionary<string, object>
            {
                { "isPremium", true },
                { "daysPlayed", 10 },
                { "coins", 500 }
            };

            var results1 = ruleEngine.ExecuteRules(inputs1);
            Console.WriteLine("1. Premium User with 10 days played (both conditions met)");
            Console.WriteLine($"   Result: Reward = {(results1.ContainsKey("reward") ? results1["reward"] : "No reward")}");
            Console.WriteLine($"           Coins = {(results1.ContainsKey("coins") ? results1["coins"] : inputs1["coins"])}");

            // Test case 2: Not eligible (not premium)
            var inputs2 = new Dictionary<string, object>
            {
                { "isPremium", false },
                { "daysPlayed", 10 },
                { "coins", 500 }
            };

            var results2 = ruleEngine.ExecuteRules(inputs2);
            Console.WriteLine("\n2. Non-Premium User with 10 days played (first condition not met)");
            Console.WriteLine($"   Result: Reward = {(results2.ContainsKey("reward") ? results2["reward"] : "No reward")}");
            Console.WriteLine($"           Coins = {inputs2["coins"]} (unchanged)");

            // Test case 3: Not eligible (insufficient days)
            var inputs3 = new Dictionary<string, object>
            {
                { "isPremium", true },
                { "daysPlayed", 5 },
                { "coins", 500 }
            };

            var results3 = ruleEngine.ExecuteRules(inputs3);
            Console.WriteLine("\n3. Premium User with 5 days played (second condition not met)");
            Console.WriteLine($"   Result: Reward = {(results3.ContainsKey("reward") ? results3["reward"] : "No reward")}");
            Console.WriteLine($"           Coins = {inputs3["coins"]} (unchanged)");
        }
        
        static void TestExpressionRule(Core.RuleEngine ruleEngine)
        {
            Console.WriteLine("\nTest 4: NCalc Expression Rule");
            Console.WriteLine("----------------------------");
            Console.WriteLine("Description: A rule that uses NCalc expressions for complex calculations");
            Console.WriteLine("Rule Type: Expression");

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

            Console.WriteLine("Rule Definition:");
            Console.WriteLine("- Condition: attackRoll > defenseRoll");
            Console.WriteLine("- Action 1: damage = baseDamage * (1 + criticalHit * 0.5) - (defenseValue * 0.2)");
            Console.WriteLine("- Action 2: isCritical = criticalHit == 1");
            Console.WriteLine("- Action 3: effectiveAttack = attackRoll - defenseRoll");
            Console.WriteLine("\nTest Cases:");

            ruleEngine.AddRule(ruleJson);

            // Test case 1: Successful attack
            var inputs1 = new Dictionary<string, object>
            {
                { "attackRoll", 15 },
                { "defenseRoll", 10 },
                { "baseDamage", 20 },
                { "defenseValue", 5 },
                { "criticalHit", 1 }  // 1 = true, 0 = false
            };

            var results1 = ruleEngine.ExecuteRules(inputs1);
            Console.WriteLine("1. Critical Hit Attack (attackRoll > defenseRoll, criticalHit = 1)");
            Console.WriteLine($"   Input: Attack Roll = {inputs1["attackRoll"]}, Defense Roll = {inputs1["defenseRoll"]}");
            Console.WriteLine($"          Base Damage = {inputs1["baseDamage"]}, Defense Value = {inputs1["defenseValue"]}");
            Console.WriteLine($"   Result: Damage = {(results1.ContainsKey("damage") ? results1["damage"] : "No damage")}");
            Console.WriteLine($"           Critical = {(results1.ContainsKey("isCritical") ? results1["isCritical"] : "Unknown")}");
            Console.WriteLine($"           Effective Attack = {(results1.ContainsKey("effectiveAttack") ? results1["effectiveAttack"] : "Unknown")}");

            // Test case 2: Failed attack
            var inputs2 = new Dictionary<string, object>
            {
                { "attackRoll", 8 },
                { "defenseRoll", 12 },
                { "baseDamage", 20 },
                { "defenseValue", 5 },
                { "criticalHit", 0 }
            };

            var results2 = ruleEngine.ExecuteRules(inputs2);
            Console.WriteLine("\n2. Failed Attack (attackRoll < defenseRoll)");
            Console.WriteLine($"   Input: Attack Roll = {inputs2["attackRoll"]}, Defense Roll = {inputs2["defenseRoll"]}");
            Console.WriteLine($"   Result: Damage = {(results2.ContainsKey("damage") ? results2["damage"] : "No damage")} (condition not met)");
            
            // Test case 3: Non-critical hit
            var inputs3 = new Dictionary<string, object>
            {
                { "attackRoll", 15 },
                { "defenseRoll", 10 },
                { "baseDamage", 20 },
                { "defenseValue", 5 },
                { "criticalHit", 0 }  // non-critical
            };

            var results3 = ruleEngine.ExecuteRules(inputs3);
            Console.WriteLine("\n3. Normal Attack (attackRoll > defenseRoll, criticalHit = 0)");
            Console.WriteLine($"   Input: Attack Roll = {inputs3["attackRoll"]}, Defense Roll = {inputs3["defenseRoll"]}");
            Console.WriteLine($"          Base Damage = {inputs3["baseDamage"]}, Defense Value = {inputs3["defenseValue"]}");
            Console.WriteLine($"   Result: Damage = {(results3.ContainsKey("damage") ? results3["damage"] : "No damage")}");
            Console.WriteLine($"           Critical = {(results3.ContainsKey("isCritical") ? results3["isCritical"] : "Unknown")}");
        }
        
        static void TestCompositeRule(Core.RuleEngine ruleEngine)
        {
            Console.WriteLine("\nTest 5: Composite Rule");
            Console.WriteLine("----------------------");
            Console.WriteLine("Description: Tests for composite rules with logical operators (AND, OR)");
            Console.WriteLine("Rule Type: Composite");

            // First composite rule (AND)
            Console.WriteLine("\nTest 5.1: AND Composite Rule");
            Console.WriteLine("--------------------------");
            Console.WriteLine("Rule Definition: Veteran Premium Player Rule (AND operator)");
            Console.WriteLine("- Child Rule 1: isPremium = true");
            Console.WriteLine("- Child Rule 2: daysPlayed >= 30");
            Console.WriteLine("- Action when both conditions met: Set specialReward and gems");

            string ruleJson = @"{
                ""ruleId"": ""veteran_premium_player"",
                ""ruleName"": ""Veteran Premium Player Rule"",
                ""type"": ""composite"",
                ""operator"": ""And"",
                ""rules"": [
                    {
                        ""ruleId"": ""is_premium"",
                        ""ruleName"": ""Is Premium Rule"",
                        ""type"": ""simple"",
                        ""conditions"": {
                            ""isPremium"": {
                                ""operator"": ""equals"",
                                ""value"": true
                            }
                        },
                        ""actions"": {}
                    },
                    {
                        ""ruleId"": ""is_veteran"",
                        ""ruleName"": ""Is Veteran Rule"",
                        ""type"": ""simple"",
                        ""conditions"": {
                            ""daysPlayed"": {
                                ""operator"": ""greaterThanOrEqual"",
                                ""value"": 30
                            }
                        },
                        ""actions"": {}
                    }
                ],
                ""actions"": {
                    ""specialReward"": {
                        ""operator"": ""set"",
                        ""value"": ""Veteran Premium Chest""
                    },
                    ""gems"": {
                        ""operator"": ""set"",
                        ""value"": 500
                    }
                }
            }";

            ruleEngine.AddRule(ruleJson);

            // Test case 1: Veteran premium player
            var inputs1 = new Dictionary<string, object>
            {
                { "isPremium", true },
                { "daysPlayed", 45 }
            };

            var results1 = ruleEngine.ExecuteRules(inputs1);
            Console.WriteLine("\nTest Case 1 - Veteran Premium Player (both conditions met):");
            Console.WriteLine($"Input: Premium = {inputs1["isPremium"]}, Days Played = {inputs1["daysPlayed"]}");
            Console.WriteLine($"Result: Special Reward = {(results1.ContainsKey("specialReward") ? results1["specialReward"] : "No reward")}");
            Console.WriteLine($"        Gems = {(results1.ContainsKey("gems") ? results1["gems"] : 0)}");

            // Test case 2: Premium but not veteran player
            var inputs2 = new Dictionary<string, object>
            {
                { "isPremium", true },
                { "daysPlayed", 20 }
            };

            var results2 = ruleEngine.ExecuteRules(inputs2);
            Console.WriteLine("\nTest Case 2 - Premium but Not Veteran Player (one condition not met):");
            Console.WriteLine($"Input: Premium = {inputs2["isPremium"]}, Days Played = {inputs2["daysPlayed"]}");
            Console.WriteLine($"Result: Special Reward = {(results2.ContainsKey("specialReward") ? results2["specialReward"] : "No reward")}");
            Console.WriteLine($"        Gems = {(results2.ContainsKey("gems") ? results2["gems"] : 0)}");

            // Second composite rule (OR)
            Console.WriteLine("\nTest 5.2: OR Composite Rule");
            Console.WriteLine("-------------------------");
            Console.WriteLine("Rule Definition: Special Offer Rule (OR operator)");
            Console.WriteLine("- Child Rule 1: daysPlayed < 7 (new player)");
            Console.WriteLine("- Child Rule 2: daysSinceLastLogin > 30 (returning player)");
            Console.WriteLine("- Action when either condition met: Set specialOffer and discount");

            string orRuleJson = @"{
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

            ruleEngine.AddRule(orRuleJson);

            // Test case 3: New player
            var inputs3 = new Dictionary<string, object>
            {
                { "daysPlayed", 3 },
                { "daysSinceLastLogin", 0 }
            };

            var results3 = ruleEngine.ExecuteRules(inputs3);
            Console.WriteLine("\nTest Case 3 - New Player (first condition met):");
            Console.WriteLine($"Input: Days Played = {inputs3["daysPlayed"]}, Days Since Last Login = {inputs3["daysSinceLastLogin"]}");
            Console.WriteLine($"Result: Special Offer = {(results3.ContainsKey("specialOffer") ? results3["specialOffer"] : "No offer")}");
            Console.WriteLine($"        Discount = {(results3.ContainsKey("discount") ? results3["discount"] : 0)}%");

            // Test case 4: Returning player
            var inputs4 = new Dictionary<string, object>
            {
                { "daysPlayed", 100 },
                { "daysSinceLastLogin", 45 }
            };

            var results4 = ruleEngine.ExecuteRules(inputs4);
            Console.WriteLine("\nTest Case 4 - Returning Player (second condition met):");
            Console.WriteLine($"Input: Days Played = {inputs4["daysPlayed"]}, Days Since Last Login = {inputs4["daysSinceLastLogin"]}");
            Console.WriteLine($"Result: Special Offer = {(results4.ContainsKey("specialOffer") ? results4["specialOffer"] : "No offer")}");
            Console.WriteLine($"        Discount = {(results4.ContainsKey("discount") ? results4["discount"] : 0)}%");

            // Test case 5: Regular player (no special offers)
            var inputs5 = new Dictionary<string, object>
            {
                { "daysPlayed", 15 },
                { "daysSinceLastLogin", 2 }
            };

            var results5 = ruleEngine.ExecuteRules(inputs5);
            Console.WriteLine("\nTest Case 5 - Regular Player (no conditions met):");
            Console.WriteLine($"Input: Days Played = {inputs5["daysPlayed"]}, Days Since Last Login = {inputs5["daysSinceLastLogin"]}");
            Console.WriteLine($"Result: Special Offer = {(results5.ContainsKey("specialOffer") ? results5["specialOffer"] : "No offer")}");
            Console.WriteLine($"        Discount = {(results5.ContainsKey("discount") ? results5["discount"] : 0)}%");
        }
    }
} 