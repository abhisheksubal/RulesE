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

            // Test case 1: Score above threshold
            var inputs1 = new Dictionary<string, object>
            {
                { "score", 1500 }
            };

            var results1 = ruleEngine.ExecuteRules(inputs1);
            Console.WriteLine($"Input Score: 1500");
            Console.WriteLine($"Bonus Applied: {(results1.ContainsKey("bonus") ? results1["bonus"] : "No bonus")}");

            // Test case 2: Score below threshold
            var inputs2 = new Dictionary<string, object>
            {
                { "score", 500 }
            };

            var results2 = ruleEngine.ExecuteRules(inputs2);
            Console.WriteLine($"Input Score: 500");
            Console.WriteLine($"Bonus Applied: {(results2.ContainsKey("bonus") ? results2["bonus"] : "No bonus")}");
        }

        static void TestLevelUpRule(Core.RuleEngine ruleEngine)
        {
            Console.WriteLine("\nTest 2: Level Up Rule");
            Console.WriteLine("--------------------");

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

            ruleEngine.AddRule(ruleJson);

            var inputs = new Dictionary<string, object>
            {
                { "experience", 1500 },
                { "level", 1 }
            };

            var results = ruleEngine.ExecuteRules(inputs);
            Console.WriteLine($"Initial Level: {inputs["level"]}");
            Console.WriteLine($"Initial Experience: {inputs["experience"]}");
            Console.WriteLine($"New Level: {(results.ContainsKey("level") ? results["level"] : inputs["level"])}");
            Console.WriteLine($"Remaining Experience: {(results.ContainsKey("experience") ? results["experience"] : inputs["experience"])}");
        }

        static void TestMultipleConditionsRule(Core.RuleEngine ruleEngine)
        {
            Console.WriteLine("\nTest 3: Multiple Conditions Rule");
            Console.WriteLine("--------------------------------");

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

            ruleEngine.AddRule(ruleJson);

            // Test case 1: Eligible for reward
            var inputs1 = new Dictionary<string, object>
            {
                { "isPremium", true },
                { "daysPlayed", 10 },
                { "coins", 500 }
            };

            var results1 = ruleEngine.ExecuteRules(inputs1);
            Console.WriteLine("Test Case 1 - Eligible:");
            Console.WriteLine($"Reward: {(results1.ContainsKey("reward") ? results1["reward"] : "No reward")}");
            Console.WriteLine($"New Coins: {(results1.ContainsKey("coins") ? results1["coins"] : inputs1["coins"])}");

            // Test case 2: Not eligible (not premium)
            var inputs2 = new Dictionary<string, object>
            {
                { "isPremium", false },
                { "daysPlayed", 10 },
                { "coins", 500 }
            };

            var results2 = ruleEngine.ExecuteRules(inputs2);
            Console.WriteLine("\nTest Case 2 - Not Eligible (Not Premium):");
            Console.WriteLine($"Reward: {(results2.ContainsKey("reward") ? results2["reward"] : "No reward")}");
            Console.WriteLine($"Coins: {inputs2["coins"]}");

            // Test case 3: Not eligible (insufficient days)
            var inputs3 = new Dictionary<string, object>
            {
                { "isPremium", true },
                { "daysPlayed", 5 },
                { "coins", 500 }
            };

            var results3 = ruleEngine.ExecuteRules(inputs3);
            Console.WriteLine("\nTest Case 3 - Not Eligible (Insufficient Days):");
            Console.WriteLine($"Reward: {(results3.ContainsKey("reward") ? results3["reward"] : "No reward")}");
            Console.WriteLine($"Coins: {inputs3["coins"]}");
        }
        
        static void TestExpressionRule(Core.RuleEngine ruleEngine)
        {
            Console.WriteLine("\nTest 4: NCalc Expression Rule");
            Console.WriteLine("----------------------------");

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
            Console.WriteLine("Test Case 1 - Successful Critical Attack:");
            Console.WriteLine($"Attack Roll: {inputs1["attackRoll"]}");
            Console.WriteLine($"Defense Roll: {inputs1["defenseRoll"]}");
            Console.WriteLine($"Base Damage: {inputs1["baseDamage"]}");
            Console.WriteLine($"Critical Hit: {(inputs1["criticalHit"].Equals(1) ? "Yes" : "No")}");
            Console.WriteLine($"Damage Dealt: {(results1.ContainsKey("damage") ? results1["damage"] : "No damage")}");
            Console.WriteLine($"Is Critical: {(results1.ContainsKey("isCritical") ? results1["isCritical"] : "Unknown")}");
            Console.WriteLine($"Effective Attack: {(results1.ContainsKey("effectiveAttack") ? results1["effectiveAttack"] : "Unknown")}");

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
            Console.WriteLine("\nTest Case 2 - Failed Attack:");
            Console.WriteLine($"Attack Roll: {inputs2["attackRoll"]}");
            Console.WriteLine($"Defense Roll: {inputs2["defenseRoll"]}");
            Console.WriteLine($"Damage Dealt: {(results2.ContainsKey("damage") ? results2["damage"] : "No damage")}");
            
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
            Console.WriteLine("\nTest Case 3 - Successful Non-Critical Attack:");
            Console.WriteLine($"Attack Roll: {inputs3["attackRoll"]}");
            Console.WriteLine($"Defense Roll: {inputs3["defenseRoll"]}");
            Console.WriteLine($"Critical Hit: {(inputs3["criticalHit"].Equals(1) ? "Yes" : "No")}");
            Console.WriteLine($"Damage Dealt: {(results3.ContainsKey("damage") ? results3["damage"] : "No damage")}");
            Console.WriteLine($"Is Critical: {(results3.ContainsKey("isCritical") ? results3["isCritical"] : "Unknown")}");
        }
        
        static void TestCompositeRule(Core.RuleEngine ruleEngine)
        {
            Console.WriteLine("\nTest 5: Composite Rule");
            Console.WriteLine("----------------------");

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
            Console.WriteLine("Test Case 1 - Veteran Premium Player:");
            Console.WriteLine($"Premium User: {inputs1["isPremium"]}");
            Console.WriteLine($"Days Played: {inputs1["daysPlayed"]}");
            Console.WriteLine($"Special Reward: {(results1.ContainsKey("specialReward") ? results1["specialReward"] : "No reward")}");
            Console.WriteLine($"Gems: {(results1.ContainsKey("gems") ? results1["gems"] : 0)}");

            // Test case 2: Premium but not veteran player
            var inputs2 = new Dictionary<string, object>
            {
                { "isPremium", true },
                { "daysPlayed", 20 }
            };

            var results2 = ruleEngine.ExecuteRules(inputs2);
            Console.WriteLine("\nTest Case 2 - Premium but Not Veteran Player:");
            Console.WriteLine($"Premium User: {inputs2["isPremium"]}");
            Console.WriteLine($"Days Played: {inputs2["daysPlayed"]}");
            Console.WriteLine($"Special Reward: {(results2.ContainsKey("specialReward") ? results2["specialReward"] : "No reward")}");
            Console.WriteLine($"Gems: {(results2.ContainsKey("gems") ? results2["gems"] : 0)}");

            // Test case 3: Or operator composite rule
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
            Console.WriteLine("\nTest Case 3 - New Player (OR condition):");
            Console.WriteLine($"Days Played: {inputs3["daysPlayed"]}");
            Console.WriteLine($"Days Since Last Login: {inputs3["daysSinceLastLogin"]}");
            Console.WriteLine($"Special Offer: {(results3.ContainsKey("specialOffer") ? results3["specialOffer"] : "No offer")}");
            Console.WriteLine($"Discount: {(results3.ContainsKey("discount") ? results3["discount"] : 0)}%");

            // Test case 4: Returning player
            var inputs4 = new Dictionary<string, object>
            {
                { "daysPlayed", 100 },
                { "daysSinceLastLogin", 45 }
            };

            var results4 = ruleEngine.ExecuteRules(inputs4);
            Console.WriteLine("\nTest Case 4 - Returning Player (OR condition):");
            Console.WriteLine($"Days Played: {inputs4["daysPlayed"]}");
            Console.WriteLine($"Days Since Last Login: {inputs4["daysSinceLastLogin"]}");
            Console.WriteLine($"Special Offer: {(results4.ContainsKey("specialOffer") ? results4["specialOffer"] : "No offer")}");
            Console.WriteLine($"Discount: {(results4.ContainsKey("discount") ? results4["discount"] : 0)}%");

            // Test case 5: Regular player (no special offers)
            var inputs5 = new Dictionary<string, object>
            {
                { "daysPlayed", 15 },
                { "daysSinceLastLogin", 2 }
            };

            var results5 = ruleEngine.ExecuteRules(inputs5);
            Console.WriteLine("\nTest Case 5 - Regular Player (no conditions met):");
            Console.WriteLine($"Days Played: {inputs5["daysPlayed"]}");
            Console.WriteLine($"Days Since Last Login: {inputs5["daysSinceLastLogin"]}");
            Console.WriteLine($"Special Offer: {(results5.ContainsKey("specialOffer") ? results5["specialOffer"] : "No offer")}");
            Console.WriteLine($"Discount: {(results5.ContainsKey("discount") ? results5["discount"] : 0)}%");
        }
    }
} 