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
    }
} 