using System.Collections.Generic;
using NUnit.Framework;
using RuleEngine.Core;

namespace RuleEngine.Tests
{
    [TestFixture]
    public class RuleEngineManualTests
    {
        private RuleEngine.Core.RuleEngine ruleEngine;

        [SetUp]
        public void Setup()
        {
            var ruleEngineBuilder = new RuleEngineBuilder();
            ruleEngine = ruleEngineBuilder.Build();
        }

        [Test]
        public void TestScoreBonusRule()
        {
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

            var inputs1 = new Dictionary<string, object> { { "score", 1500 } };
            var results1 = ruleEngine.ExecuteRules(inputs1);
            Assert.IsTrue(results1.ContainsKey("bonus"));
            Assert.AreEqual(1.5, results1["bonus"]);

            var inputs2 = new Dictionary<string, object> { { "score", 500 } };
            var results2 = ruleEngine.ExecuteRules(inputs2);
            Assert.IsFalse(results2.ContainsKey("bonus"));
        }

        [Test]
        public void TestLevelUpRule()
        {
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

            var inputs = new Dictionary<string, object> { { "experience", 1500 }, { "level", 1 } };
            var results = ruleEngine.ExecuteRules(inputs);
            Assert.IsTrue(results.ContainsKey("level"));
            Assert.AreEqual(2, results["level"]);
            Assert.IsTrue(results.ContainsKey("experience"));
            Assert.AreEqual(500, results["experience"]);
        }

        [Test]
        public void TestMultipleConditionsRule()
        {
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

            var inputs1 = new Dictionary<string, object> { { "isPremium", true }, { "daysPlayed", 10 }, { "coins", 500 } };
            var results1 = ruleEngine.ExecuteRules(inputs1);
            Assert.IsTrue(results1.ContainsKey("reward"));
            Assert.AreEqual("Premium Chest", results1["reward"]);
            Assert.IsTrue(results1.ContainsKey("coins"));
            Assert.AreEqual(1500, results1["coins"]);

            var inputs2 = new Dictionary<string, object> { { "isPremium", false }, { "daysPlayed", 10 }, { "coins", 500 } };
            var results2 = ruleEngine.ExecuteRules(inputs2);
            Assert.IsFalse(results2.ContainsKey("reward"));
            Assert.AreEqual(500, results2["coins"]);

            var inputs3 = new Dictionary<string, object> { { "isPremium", true }, { "daysPlayed", 5 }, { "coins", 500 } };
            var results3 = ruleEngine.ExecuteRules(inputs3);
            Assert.IsFalse(results3.ContainsKey("reward"));
            Assert.AreEqual(500, results3["coins"]);
        }
    }
} 