using System;
using System.Collections.Generic;
using NUnit.Framework;
using RuleEngine.Core;
using RuleEngine.Rules.Factories;
using Newtonsoft.Json;

namespace RuleEngine.Tests
{
    [TestFixture]
    public class RuleTypesTests
    {
        private RuleEngineBuilder _builder;
        private Core.RuleEngine _engine;

        [SetUp]
        public void Setup()
        {
            _builder = new RuleEngineBuilder();
            _engine = _builder.Build();
        }

        [Test]
        public void TestSimpleRule()
        {
            // Arrange
            var ruleJson = @"{
                ""ruleId"": ""simple_rule_1"",
                ""ruleName"": ""Simple Score Rule"",
                ""type"": ""simple"",
                ""conditions"": {
                    ""score"": {
                        ""operator"": ""greaterThan"",
                        ""value"": 100
                    }
                },
                ""actions"": {
                    ""bonus"": {
                        ""operator"": ""set"",
                        ""value"": 1.5
                    }
                }
            }";

            _engine.AddRule(ruleJson);

            // Act
            var inputs = new Dictionary<string, object>
            {
                { "score", 150 }
            };
            var results = _engine.ExecuteRules(inputs);

            // Assert
            Assert.That(results.ContainsKey("bonus"), Is.True);
            Assert.That(results["bonus"], Is.EqualTo(1.5));
        }

        [Test]
        public void TestExpressionRule()
        {
            // Arrange
            var ruleJson = @"{
                ""ruleId"": ""expression_rule_1"",
                ""ruleName"": ""Expression Score Rule"",
                ""type"": ""expression"",
                ""conditionExpression"": ""score > 100 && level >= 5"",
                ""actionExpressions"": {
                    ""bonus"": ""score * 0.1"",
                    ""message"": ""'Congratulations! You earned a bonus!'""
                }
            }";

            _engine.AddRule(ruleJson);

            // Act
            var inputs = new Dictionary<string, object>
            {
                { "score", 150 },
                { "level", 6 }
            };
            var results = _engine.ExecuteRules(inputs);

            // Assert
            Assert.That(results.ContainsKey("bonus"), Is.True);
            Assert.That(results["bonus"], Is.EqualTo(15.0));
            Assert.That(results.ContainsKey("message"), Is.True);
            Assert.That(results["message"], Is.EqualTo("Congratulations! You earned a bonus!"));
        }

        [Test]
        public void TestExpressionRuleCallback()
        {
            // Arrange
            var ruleJson = @"{
                ""ruleId"": ""expression_callback_rule"",
                ""ruleName"": ""Expression Callback Rule"",
                ""type"": ""expression"",
                ""conditionExpression"": ""eventType == 'spin_gained'"",
                ""actionExpressions"": {
                    ""notify"": ""=> spin_collected""
                }
            }";

            _engine.AddRule(ruleJson);

            // Act
            var inputs = new Dictionary<string, object>
            {
                { "eventType", "spin_gained" }
            };
            var results = _engine.ExecuteRules(inputs);

            // Assert
            Assert.That(results.ContainsKey("__callbacks__"), Is.True);
            var callbacks = results["__callbacks__"] as List<Dictionary<string, object>>;
            Assert.That(callbacks, Is.Not.Null);
            Assert.That(callbacks.Count, Is.EqualTo(1));
            Assert.That(callbacks[0]["name"], Is.EqualTo("notify"));
            Assert.That(callbacks[0]["value"], Is.EqualTo("spin_collected"));
            // No data mutation for callback
            Assert.That(results.ContainsKey("notify"), Is.False);
        }

        [Test]
        public void TestExpressionRuleCallbackWithFullOperator()
        {
            // Arrange
            var ruleJson = @"{
                ""ruleId"": ""expression_callback_rule_full"",
                ""ruleName"": ""Expression Callback Rule with Full Operator"",
                ""type"": ""expression"",
                ""conditionExpression"": ""eventType == 'spin_gained'"",
                ""actionExpressions"": {
                    ""notify"": ""callback(spin_collected)""
                }
            }";

            _engine.AddRule(ruleJson);

            // Act
            var inputs = new Dictionary<string, object>
            {
                { "eventType", "spin_gained" }
            };
            var results = _engine.ExecuteRules(inputs);

            // Assert
            Assert.That(results.ContainsKey("__callbacks__"), Is.True);
            var callbacks = results["__callbacks__"] as List<Dictionary<string, object>>;
            Assert.That(callbacks, Is.Not.Null);
            Assert.That(callbacks.Count, Is.EqualTo(1));
            Assert.That(callbacks[0]["name"], Is.EqualTo("notify"));
            Assert.That(callbacks[0]["value"], Is.EqualTo("spin_collected"));
            // No data mutation for callback
            Assert.That(results.ContainsKey("notify"), Is.False);
        }

        [Test]
        public void TestCompositeRule_And()
        {
            // Arrange
            var ruleJson = @"{
                ""ruleId"": ""composite_rule_1"",
                ""ruleName"": ""Composite And Rule"",
                ""type"": ""composite"",
                ""operator"": ""And"",
                ""rules"": [
                    {
                        ""ruleId"": ""sub_rule_1"",
                        ""ruleName"": ""Score Check"",
                        ""type"": ""simple"",
                        ""conditions"": {
                            ""score"": {
                                ""operator"": ""greaterThan"",
                                ""value"": 100
                            }
                        },
                        ""actions"": {
                            ""scoreBonus"": {
                                ""operator"": ""set"",
                                ""value"": 1.5
                            }
                        }
                    },
                    {
                        ""ruleId"": ""sub_rule_2"",
                        ""ruleName"": ""Level Check"",
                        ""type"": ""simple"",
                        ""conditions"": {
                            ""level"": {
                                ""operator"": ""greaterThanOrEqual"",
                                ""value"": 5
                            }
                        },
                        ""actions"": {
                            ""levelBonus"": {
                                ""operator"": ""set"",
                                ""value"": 2.0
                            }
                        }
                    }
                ],
                ""actions"": {
                    ""finalBonus"": {
                        ""operator"": ""set"",
                        ""value"": 3.0
                    }
                }
            }";

            _engine.AddRule(ruleJson);

            // Act
            var inputs = new Dictionary<string, object>
            {
                { "score", 150 },
                { "level", 6 }
            };
            var results = _engine.ExecuteRules(inputs);

            // Assert
            Assert.That(results.ContainsKey("scoreBonus"), Is.True);
            Assert.That(results.ContainsKey("levelBonus"), Is.True);
            Assert.That(results.ContainsKey("finalBonus"), Is.True);
            Assert.That(results["scoreBonus"], Is.EqualTo(1.5));
            Assert.That(results["levelBonus"], Is.EqualTo(2.0));
            Assert.That(results["finalBonus"], Is.EqualTo(3.0));
        }

        [Test]
        public void TestCompositeRule_Or()
        {
            // Arrange
            var ruleJson = @"{
                ""ruleId"": ""composite_rule_2"",
                ""ruleName"": ""Composite Or Rule"",
                ""type"": ""composite"",
                ""operator"": ""Or"",
                ""rules"": [
                    {
                        ""ruleId"": ""sub_rule_1"",
                        ""ruleName"": ""Score Check"",
                        ""type"": ""simple"",
                        ""conditions"": {
                            ""score"": {
                                ""operator"": ""greaterThan"",
                                ""value"": 100
                            }
                        },
                        ""actions"": {
                            ""scoreBonus"": {
                                ""operator"": ""set"",
                                ""value"": 1.5
                            }
                        }
                    },
                    {
                        ""ruleId"": ""sub_rule_2"",
                        ""ruleName"": ""Level Check"",
                        ""type"": ""simple"",
                        ""conditions"": {
                            ""level"": {
                                ""operator"": ""greaterThanOrEqual"",
                                ""value"": 5
                            }
                        },
                        ""actions"": {
                            ""levelBonus"": {
                                ""operator"": ""set"",
                                ""value"": 2.0
                            }
                        }
                    }
                ]
            }";

            _engine.AddRule(ruleJson);

            // Act
            var inputs = new Dictionary<string, object>
            {
                { "score", 50 },  // Below threshold
                { "level", 6 }    // Above threshold
            };
            var results = _engine.ExecuteRules(inputs);

            // Assert
            Assert.That(results.ContainsKey("scoreBonus"), Is.False);
            Assert.That(results.ContainsKey("levelBonus"), Is.True);
            Assert.That(results["levelBonus"], Is.EqualTo(2.0));
        }

        [Test]
        public void TestCompositeRule_Not()
        {
            // Arrange
            var ruleJson = @"{
                ""ruleId"": ""composite_rule_3"",
                ""ruleName"": ""Composite Not Rule"",
                ""type"": ""composite"",
                ""operator"": ""Not"",
                ""rules"": [
                    {
                        ""ruleId"": ""sub_rule_1"",
                        ""ruleName"": ""Score Check"",
                        ""type"": ""simple"",
                        ""conditions"": {
                            ""score"": {
                                ""operator"": ""lessThan"",
                                ""value"": 50
                            }
                        },
                        ""actions"": {
                            ""message"": {
                                ""operator"": ""set"",
                                ""value"": ""Score is too low""
                            }
                        }
                    }
                ],
                ""actions"": {
                    ""message"": {
                        ""operator"": ""set"",
                        ""value"": ""Score is good""
                    }
                }
            }";

            _engine.AddRule(ruleJson);

            // Act
            var inputs = new Dictionary<string, object>
            {
                { "score", 75 }  // Above threshold
            };
            var results = _engine.ExecuteRules(inputs);

            // Assert
            Assert.That(results.ContainsKey("message"), Is.True);
            Assert.That(results["message"], Is.EqualTo("Score is good"));
        }

        [Test]
        public void TestMultipleRules()
        {
            // Arrange
            var simpleRuleJson = @"{
                ""ruleId"": ""simple_rule_2"",
                ""ruleName"": ""Simple Level Rule"",
                ""type"": ""simple"",
                ""conditions"": {
                    ""level"": {
                        ""operator"": ""greaterThan"",
                        ""value"": 10
                    }
                },
                ""actions"": {
                    ""levelBonus"": {
                        ""operator"": ""set"",
                        ""value"": 2.0
                    }
                }
            }";

            var expressionRuleJson = @"{
                ""ruleId"": ""expression_rule_2"",
                ""ruleName"": ""Expression Score Rule"",
                ""type"": ""expression"",
                ""conditionExpression"": ""score > 200"",
                ""actionExpressions"": {
                    ""scoreBonus"": ""score * 0.2""
                }
            }";

            _engine.AddRule(simpleRuleJson);
            _engine.AddRule(expressionRuleJson);

            // Act
            var inputs = new Dictionary<string, object>
            {
                { "score", 250 },
                { "level", 15 }
            };
            var results = _engine.ExecuteRules(inputs);

            // Assert
            Assert.That(results.ContainsKey("levelBonus"), Is.True);
            Assert.That(results.ContainsKey("scoreBonus"), Is.True);
            Assert.That(results["levelBonus"], Is.EqualTo(2.0));
            Assert.That(results["scoreBonus"], Is.EqualTo(50.0));
        }

        [Test]
        public void TestInputModification()
        {
            // Arrange
            var ruleJson = @"{
                ""ruleId"": ""input_mod_rule"",
                ""ruleName"": ""Input Modification Rule"",
                ""type"": ""simple"",
                ""conditions"": {
                    ""score"": {
                        ""operator"": ""greaterThan"",
                        ""value"": 50
                    }
                },
                ""actions"": {
                    ""newScore"": {
                        ""operator"": ""add"",
                        ""value"": 10
                    },
                    ""newLevel"": {
                        ""operator"": ""set"",
                        ""value"": ""score""
                    },
                    ""newBonus"": {
                        ""operator"": ""multiply"",
                        ""value"": 2
                    },
                    ""message"": {
                        ""operator"": ""set"",
                        ""value"": ""Level up!""
                    }
                }
            }";

            _engine.AddRule(ruleJson);

            // Act
            var inputs = new Dictionary<string, object>
            {
                { "score", 60 },
                { "level", 1 },
                { "bonus", 5 }
            };
            var results = _engine.ExecuteRules(inputs);

            // Assert
            Assert.That(results.ContainsKey("newScore"), Is.True);
            Assert.That(results["newScore"], Is.EqualTo(70.0)); // 60 + 10
            Assert.That(results.ContainsKey("newLevel"), Is.True);
            Assert.That(results["newLevel"], Is.EqualTo(60.0)); // Set to score value
            Assert.That(results.ContainsKey("newBonus"), Is.True);
            Assert.That(results["newBonus"], Is.EqualTo(10.0)); // 5 * 2
            Assert.That(results.ContainsKey("message"), Is.True);
            Assert.That(results["message"], Is.EqualTo("Level up!"));
        }

        [Test]
        public void TestInputModificationWithExpressions()
        {
            // Arrange
            var ruleJson = @"{
                ""ruleId"": ""input_mod_expr_rule"",
                ""ruleName"": ""Input Modification Expression Rule"",
                ""type"": ""expression"",
                ""conditionExpression"": ""score > 50 && level < 10"",
                ""actionExpressions"": {
                    ""newScore"": ""score + 10"",
                    ""newLevel"": ""score / 10"",
                    ""newBonus"": ""bonus * 2"",
                    ""message"": ""'Level up to ' + (score / 10) + '!'""
                }
            }";

            _engine.AddRule(ruleJson);

            // Act
            var inputs = new Dictionary<string, object>
            {
                { "score", 60 },
                { "level", 5 },
                { "bonus", 5 }
            };
            var results = _engine.ExecuteRules(inputs);

            // Assert
            Assert.That(results.ContainsKey("newScore"), Is.True);
            Assert.That(results["newScore"], Is.EqualTo(70.0)); // 60 + 10
            Assert.That(results.ContainsKey("newLevel"), Is.True);
            Assert.That(results["newLevel"], Is.EqualTo(6.0)); // 60 / 10
            Assert.That(results.ContainsKey("newBonus"), Is.True);
            Assert.That(results["newBonus"], Is.EqualTo(10.0)); // 5 * 2
            Assert.That(results.ContainsKey("message"), Is.True);
            Assert.That(results["message"], Is.EqualTo("Level up to 6!"));
        }

        [Test]
        public void TestInputModificationWithCompositeRules()
        {
            // Arrange
            var ruleJson = @"{
                ""ruleId"": ""input_mod_composite_rule"",
                ""ruleName"": ""Input Modification Composite Rule"",
                ""type"": ""composite"",
                ""operator"": ""And"",
                ""rules"": [
                    {
                        ""ruleId"": ""score_rule"",
                        ""ruleName"": ""Score Rule"",
                        ""type"": ""simple"",
                        ""conditions"": {
                            ""score"": {
                                ""operator"": ""greaterThan"",
                                ""value"": 50
                            }
                        },
                        ""actions"": {
                            ""newScore"": {
                                ""operator"": ""add"",
                                ""value"": 10
                            }
                        }
                    },
                    {
                        ""ruleId"": ""level_rule"",
                        ""ruleName"": ""Level Rule"",
                        ""type"": ""expression"",
                        ""conditionExpression"": ""score > 60"",
                        ""actionExpressions"": {
                            ""newLevel"": ""score / 10"",
                            ""newBonus"": ""bonus * 2""
                        }
                    }
                ],
                ""actions"": {
                    ""message"": {
                        ""operator"": ""set"",
                        ""value"": ""Level up!""
                    }
                }
            }";

            _engine.AddRule(ruleJson);

            // Act
            var inputs = new Dictionary<string, object>
            {
                { "score", 60 },
                { "level", 5 },
                { "bonus", 5 }
            };
            var results = _engine.ExecuteRules(inputs);

            // Assert
            Assert.That(results.ContainsKey("newScore"), Is.True);
            Assert.That(results["newScore"], Is.EqualTo(70.0)); // 60 + 10
            Assert.That(results.ContainsKey("newLevel"), Is.True);
            Assert.That(results["newLevel"], Is.EqualTo(6.0)); // 60 / 10
            Assert.That(results.ContainsKey("newBonus"), Is.True);
            Assert.That(results["newBonus"], Is.EqualTo(10.0)); // 5 * 2
            Assert.That(results.ContainsKey("message"), Is.True);
            Assert.That(results["message"], Is.EqualTo("Level up!"));
        }

        [Test]
        public void TestCallbackAction()
        {
            // Arrange
            var ruleJson = @"{
                ""ruleId"": ""callback_rule"",
                ""ruleName"": ""Callback Rule"",
                ""type"": ""simple"",
                ""conditions"": {
                    ""eventType"": { ""operator"": ""equals"", ""value"": ""spin_gained"" }
                },
                ""actions"": {
                    ""notify"": { ""operator"": ""callback"", ""value"": ""spin_collected"" }
                }
            }";

            _engine.AddRule(ruleJson);

            // Act
            var inputs = new Dictionary<string, object>
            {
                { "eventType", "spin_gained" }
            };
            var results = _engine.ExecuteRules(inputs);

            // Assert
            Assert.That(results.ContainsKey("__callbacks__"), Is.True);
            var callbacks = results["__callbacks__"] as List<Dictionary<string, object>>;
            Assert.That(callbacks, Is.Not.Null);
            Assert.That(callbacks.Count, Is.EqualTo(1));
            Assert.That(callbacks[0]["name"], Is.EqualTo("notify"));
            Assert.That(callbacks[0]["value"], Is.EqualTo("spin_collected"));
            // No data mutation for callback
            Assert.That(results.ContainsKey("notify"), Is.False);
        }
    }
} 