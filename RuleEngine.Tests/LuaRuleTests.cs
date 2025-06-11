using NUnit.Framework;
using RuleEngine.Core;
using RuleEngine.Rules.Factories;

namespace RuleEngine.Tests
{
    [TestFixture]
    public class LuaRuleTests
    {
        private Core.RuleEngine _ruleEngine;
        private JsonRuleParser _parser;

        [SetUp]
        public void Setup()
        {
            var factoryRegistry = new RuleFactoryRegistry();
            factoryRegistry.RegisterFactory(new LuaRuleFactory());
            _parser = new JsonRuleParser(factoryRegistry);
            _ruleEngine = new Core.RuleEngine(_parser);
        }

        [Test]
        public void CreateLuaRule_WithValidDefinition_Success()
        {
            // Arrange
            var ruleJson = @"{
                ""ruleId"": ""test_rule"",
                ""ruleName"": ""Test Rule"",
                ""type"": ""lua"",
                ""conditionExpression"": ""return value > 10"",
                ""actionExpressions"": {
                    ""result"": ""return value * 2""
                }
            }";

            // Act
            var rule = _parser.Parse(ruleJson);

            // Assert
            Assert.That(rule, Is.Not.Null);
            Assert.That(rule.RuleId, Is.EqualTo("test_rule"));
            Assert.That(rule.RuleName, Is.EqualTo("Test Rule"));
        }

        [Test]
        public void CreateLuaRule_WithInvalidDefinition_DoesNotThrowException()
        {
            // Arrange
            var ruleJson = @"{
                ""ruleId"": ""test_rule"",
                ""ruleName"": ""Test Rule"",
                ""type"": ""lua"",
                ""conditionExpression"": ""return invalid expression"",
                ""actionExpressions"": {
                    ""result"": ""return value * 2""
                }
            }";

            // Act & Assert
            Assert.DoesNotThrow(() => _parser.Parse(ruleJson));
        }

        [TestCase("return value > 10", 15, true)]
        [TestCase("return value > 10", 5, false)]
        [TestCase("return value >= 10", 10, true)]
        [TestCase("return value < 10", 5, true)]
        [TestCase("return value <= 10", 10, true)]
        [TestCase("return value == 10", 10, true)]
        [TestCase("return value ~= 10", 5, true)]
        public void EvaluateLuaRule_WithComparisonOperators_ReturnsExpectedResult(
            string conditionExpression, int value, bool expectedResult)
        {
            // Arrange
            var ruleJson = $@"{{
                ""ruleId"": ""test_rule"",
                ""ruleName"": ""Test Rule"",
                ""type"": ""lua"",
                ""conditionExpression"": ""{conditionExpression}"",
                ""actionExpressions"": {{
                    ""result"": ""return value * 2""
                }}
            }}";

            var rule = _parser.Parse(ruleJson);
            var inputs = new Dictionary<string, object> { { "value", value } };

            // Act
            var result = rule.Evaluate(inputs);

            // Assert
            Assert.That(result, Is.EqualTo(expectedResult));
        }

        [TestCase("return value1 > 10 and value2 < 20", 15, 15, true)]
        [TestCase("return value1 > 10 and value2 < 20", 5, 15, false)]
        [TestCase("return value1 > 10 or value2 < 20", 5, 15, true)]
        [TestCase("return not (value1 > 10)", 15, 0, false)]
        public void EvaluateLuaRule_WithLogicalOperators_ReturnsExpectedResult(
            string conditionExpression, int value1, int value2, bool expectedResult)
        {
            // Arrange
            var ruleJson = $@"{{
                ""ruleId"": ""test_rule"",
                ""ruleName"": ""Test Rule"",
                ""type"": ""lua"",
                ""conditionExpression"": ""{conditionExpression}"",
                ""actionExpressions"": {{
                    ""result"": ""return value1 + value2""
                }}
            }}";

            var rule = _parser.Parse(ruleJson);
            var inputs = new Dictionary<string, object>
            {
                { "value1", value1 },
                { "value2", value2 }
            };

            // Act
            var result = rule.Evaluate(inputs);

            // Assert
            Assert.That(result, Is.EqualTo(expectedResult));
        }

        [TestCase("return value + 5", 10, 15.0)]
        [TestCase("return value - 5", 10, 5.0)]
        [TestCase("return value * 2", 10, 20.0)]
        [TestCase("return value / 2", 10, 5.0)]
        [TestCase("return value % 3", 10, 1.0)]
        public void ExecuteLuaRule_WithArithmeticOperators_ReturnsExpectedResult(
            string actionExpression, int value, double expectedResult)
        {
            // Arrange
            var ruleJson = $@"{{
                ""ruleId"": ""test_rule"",
                ""ruleName"": ""Test Rule"",
                ""type"": ""lua"",
                ""conditionExpression"": ""return true"",
                ""actionExpressions"": {{
                    ""result"": ""{actionExpression}""
                }}
            }}";

            var rule = _parser.Parse(ruleJson);
            var inputs = new Dictionary<string, object> { { "value", value } };

            // Act
            var results = rule.Execute(inputs);

            // Assert
            Assert.That(Convert.ToDouble(results["result"]), Is.EqualTo(expectedResult));
        }

        [Test]
        public void ExecuteLuaRule_WithMultipleActionExpressions_ReturnsAllResults()
        {
            // Arrange
            var ruleJson = @"{
                ""ruleId"": ""test_rule"",
                ""ruleName"": ""Test Rule"",
                ""type"": ""lua"",
                ""conditionExpression"": ""return true"",
                ""actionExpressions"": {
                    ""sum"": ""return value1 + value2"",
                    ""product"": ""return value1 * value2"",
                    ""average"": ""return (value1 + value2) / 2""
                }
            }";

            var rule = _parser.Parse(ruleJson);
            var inputs = new Dictionary<string, object>
            {
                { "value1", 10 },
                { "value2", 20 }
            };

            // Act
            var results = rule.Execute(inputs);

            // Assert
            Assert.That(Convert.ToDouble(results["sum"]), Is.EqualTo(30.0));
            Assert.That(Convert.ToDouble(results["product"]), Is.EqualTo(200.0));
            Assert.That(Convert.ToDouble(results["average"]), Is.EqualTo(15.0));
        }

        [Test]
        public void ExecuteLuaRule_WithStringOperations_ReturnsExpectedResult()
        {
            // Arrange
            var ruleJson = @"{
                ""ruleId"": ""string_rule"",
                ""ruleName"": ""String Operations Rule"",
                ""type"": ""lua"",
                ""conditionExpression"": ""return text ~= ''"",
                ""actionExpressions"": {
                    ""concatenated"": ""return text .. ' World'"",
                    ""repeated"": ""return text .. text"",
                    ""contains"": ""return text == 'Hello'"",
                    ""equals"": ""return text == 'Hello'"",
                    ""notEquals"": ""return text ~= 'World'""
                }
            }";

            var rule = _parser.Parse(ruleJson);
            var inputs = new Dictionary<string, object>
            {
                { "text", "Hello" }
            };

            // Act
            var results = rule.Execute(inputs);

            // Assert
            Assert.That(results.ContainsKey("concatenated"), Is.True);
            Assert.That(results.ContainsKey("repeated"), Is.True);
            Assert.That(results.ContainsKey("contains"), Is.True);
            Assert.That(results.ContainsKey("equals"), Is.True);
            Assert.That(results.ContainsKey("notEquals"), Is.True);

            // Verify the results
            Assert.That(results["concatenated"], Is.EqualTo("Hello World"));
            Assert.That(results["repeated"], Is.EqualTo("HelloHello"));
            Assert.That(Convert.ToBoolean(results["contains"]), Is.True);
            Assert.That(Convert.ToBoolean(results["equals"]), Is.True);
            Assert.That(Convert.ToBoolean(results["notEquals"]), Is.True);
        }

        [Test]
        public void ExecuteLuaRule_WithConditionalExpressions_ReturnsExpectedResult()
        {
            // Arrange
            var ruleJson = @"{
                ""ruleId"": ""test_rule"",
                ""ruleName"": ""Test Rule"",
                ""type"": ""lua"",
                ""conditionExpression"": ""return true"",
                ""actionExpressions"": {
                    ""result"": ""return score >= 60 and 'Pass' or 'Fail'"",
                    ""grade"": ""return score >= 90 and 'A' or (score >= 80 and 'B' or (score >= 70 and 'C' or (score >= 60 and 'D' or 'F')))""
                }
            }";

            var rule = _parser.Parse(ruleJson);
            var inputs = new Dictionary<string, object> { { "score", 85 } };

            // Act
            var results = rule.Execute(inputs);

            // Assert
            Assert.That(results["result"], Is.EqualTo("Pass"));
            Assert.That(results["grade"], Is.EqualTo("B"));
        }

        [Test]
        public void ExecuteLuaRule_WithMathFunctions_ReturnsExpectedResult()
        {
            // Arrange
            var ruleJson = @"{
                ""ruleId"": ""test_rule"",
                ""ruleName"": ""Test Rule"",
                ""type"": ""lua"",
                ""conditionExpression"": ""return true"",
                ""actionExpressions"": {
                    ""abs"": ""return math.abs(-10)"",
                    ""round"": ""return math.floor(3.14159 * 100 + 0.5) / 100"",
                    ""max"": ""return math.max(10, 20)"",
                    ""min"": ""return math.min(10, 20)"",
                    ""sqrt"": ""return math.sqrt(16)""
                }
            }";

            var rule = _parser.Parse(ruleJson);
            var inputs = new Dictionary<string, object>();

            // Act
            var results = rule.Execute(inputs);

            // Assert
            Assert.That(Convert.ToDouble(results["abs"]), Is.EqualTo(10.0));
            Assert.That(Convert.ToDouble(results["round"]), Is.EqualTo(3.14));
            Assert.That(Convert.ToDouble(results["max"]), Is.EqualTo(20.0));
            Assert.That(Convert.ToDouble(results["min"]), Is.EqualTo(10.0));
            Assert.That(Convert.ToDouble(results["sqrt"]), Is.EqualTo(4.0));
        }

        [Test]
        public void ExecuteLuaRule_WithMissingInputs_ReturnsEmptyDictionary()
        {
            // Arrange
            var ruleJson = @"{
                ""ruleId"": ""test_rule"",
                ""ruleName"": ""Test Rule"",
                ""type"": ""lua"",
                ""conditionExpression"": ""return value > 10"",
                ""actionExpressions"": {
                    ""result"": ""return value * 2""
                }
            }";

            var rule = _parser.Parse(ruleJson);
            var inputs = new Dictionary<string, object>();

            // Act
            var results = rule.Execute(inputs);

            // Assert
            Assert.That(results, Is.Empty);
        }

        [Test]
        public void ExecuteLuaRule_WithInvalidExpression_ThrowsInvalidOperationException()
        {
            // Arrange
            var ruleJson = @"{
                ""ruleId"": ""test_rule"",
                ""ruleName"": ""Test Rule"",
                ""type"": ""lua"",
                ""conditionExpression"": ""return true"",
                ""actionExpressions"": {
                    ""result"": ""return invalid expression""
                }
            }";

            var rule = _parser.Parse(ruleJson);
            var inputs = new Dictionary<string, object>();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => rule.Execute(inputs));
        }

        [Test]
        public void ExecuteLuaRule_WithNullValues_HandlesGracefully()
        {
            // Arrange
            var ruleJson = @"{
                ""ruleId"": ""test_rule"",
                ""ruleName"": ""Test Rule"",
                ""type"": ""lua"",
                ""conditionExpression"": ""return true"",
                ""actionExpressions"": {
                    ""nullCheck"": ""return value == nil"",
                    ""defaultValue"": ""return value or 'default'"",
                    ""nullArithmetic"": ""return (value or 0) + 5""
                }
            }";

            var rule = _parser.Parse(ruleJson);
            var inputs = new Dictionary<string, object>
            {
                { "value", null }
            };

            // Act
            var results = rule.Execute(inputs);

            // Assert
            Assert.That(results["nullCheck"], Is.EqualTo(true));
            Assert.That(results["defaultValue"], Is.EqualTo("default"));
            Assert.That(Convert.ToDouble(results["nullArithmetic"]), Is.EqualTo(5.0));
        }

        [Test]
        public void ExecuteLuaRule_WithArrayOperations_ReturnsExpectedResult()
        {
            // Arrange
            var ruleJson = @"{
                ""ruleId"": ""array_rule"",
                ""ruleName"": ""Array Processing Rule"",
                ""type"": ""lua"",
                ""conditionExpression"": ""return scores[1] + scores[2] + scores[3] + scores[4] + scores[5] > 250"",
                ""actionExpressions"": {
                    ""totalScores"": ""return scores[1] + scores[2] + scores[3] + scores[4] + scores[5]"",
                    ""scoreCount"": ""return 5"",
                    ""hasHighScores"": ""return scores[4] >= 80"",
                    ""hasLowScores"": ""return scores[1] < 50"",
                    ""firstScore"": ""return scores[1]"",
                    ""lastScore"": ""return scores[5]"",
                    ""middleScore"": ""return scores[3]""
                }
            }";

            var rule = _parser.Parse(ruleJson);
            var inputs = new Dictionary<string, object>
            {
                { "scores", new[] { 45, 60, 75, 80, 55 } }
            };

            // Act
            var results = rule.Execute(inputs);

            // Assert
            Assert.That(results.ContainsKey("totalScores"), Is.True);
            Assert.That(results.ContainsKey("scoreCount"), Is.True);
            Assert.That(results.ContainsKey("hasHighScores"), Is.True);
            Assert.That(results.ContainsKey("hasLowScores"), Is.True);
            Assert.That(results.ContainsKey("firstScore"), Is.True);
            Assert.That(results.ContainsKey("lastScore"), Is.True);
            Assert.That(results.ContainsKey("middleScore"), Is.True);

            // Verify the results
            Assert.That(Convert.ToDouble(results["totalScores"]), Is.EqualTo(315.0)); // 45 + 60 + 75 + 80 + 55
            Assert.That(Convert.ToInt32(results["scoreCount"]), Is.EqualTo(5));
            Assert.That(Convert.ToBoolean(results["hasHighScores"]), Is.True);
            Assert.That(Convert.ToBoolean(results["hasLowScores"]), Is.True);
            Assert.That(Convert.ToDouble(results["firstScore"]), Is.EqualTo(45.0));
            Assert.That(Convert.ToDouble(results["lastScore"]), Is.EqualTo(55.0));
            Assert.That(Convert.ToDouble(results["middleScore"]), Is.EqualTo(75.0));
        }

        [Test]
        public void ExecuteLuaRule_WithCallbackAndArrayGet_ReturnsExpectedResult()
        {
            // Arrange
            var ruleJson = @"{
                ""ruleId"": ""callback_array_get_rule"",
                ""ruleName"": ""Callback and Array Get Rule"",
                ""type"": ""lua"",
                ""conditionExpression"": ""return arr[1] > 5"",
                ""actionExpressions"": {
                    ""result"": ""return arr[2]"",
                    ""notify"": ""=> spin_collected""
                }
            }";

            var rule = _parser.Parse(ruleJson);
            var inputs = new Dictionary<string, object>
            {
                { "arr", new object[] { 10, 20, 30 } }
            };

            // Act
            var results = rule.Execute(inputs);

            // Assert
            Assert.That(results["result"], Is.EqualTo(20));
            Assert.That(results.ContainsKey("__callbacks__"), Is.True);
            var callbacks = results["__callbacks__"] as List<Dictionary<string, object>>;
            Assert.That(callbacks, Is.Not.Null);
            Assert.That(callbacks.Count, Is.EqualTo(1));
            Assert.That(callbacks[0]["name"], Is.EqualTo("notify"));
            Assert.That(callbacks[0]["value"], Is.EqualTo("spin_collected"));
        }

        [Test]
        public void ExecuteLuaRule_WithCalculatedValueInCallback_ReturnsExpectedResult()
        {
            // Arrange
            var ruleJson = @"{
                ""ruleId"": ""calculated_callback_rule"",
                ""ruleName"": ""Calculated Callback Rule"",
                ""type"": ""lua"",
                ""conditionExpression"": ""return value > 10"",
                ""actionExpressions"": {
                    ""calculatedValue"": ""return value * 2"",
                    ""notify"": ""callback(calculatedValue)""
                }
            }";

            var rule = _parser.Parse(ruleJson);
            var inputs = new Dictionary<string, object>
            {
                { "value", 15 }
            };

            // Act
            var results = rule.Execute(inputs);

            // Assert
            Assert.That(results["calculatedValue"], Is.EqualTo(30)); // 15 * 2
            Assert.That(results.ContainsKey("__callbacks__"), Is.True);
            var callbacks = results["__callbacks__"] as List<Dictionary<string, object>>;
            Assert.That(callbacks, Is.Not.Null);
            Assert.That(callbacks.Count, Is.EqualTo(1));
            Assert.That(callbacks[0]["name"], Is.EqualTo("notify"));
            Assert.That(callbacks[0]["value"], Is.EqualTo(30)); // Should contain the actual calculated value
        }

        [Test]
        public void ExecuteLuaRule_WithMultipleCalculatedValuesInCallback_ReturnsExpectedResult()
        {
            // Arrange
            var ruleJson = @"{
                ""ruleId"": ""multiple_calculated_callback_rule"",
                ""ruleName"": ""Multiple Calculated Callback Rule"",
                ""type"": ""lua"",
                ""conditionExpression"": ""return value > 10"",
                ""actionExpressions"": {
                    ""sum"": ""return value + 5"",
                    ""product"": ""return value * 2"",
                    ""notify"": ""callback(sum)"",
                    ""alert"": ""callback(product)""
                }
            }";

            var rule = _parser.Parse(ruleJson);
            var inputs = new Dictionary<string, object>
            {
                { "value", 15 }
            };

            // Act
            var results = rule.Execute(inputs);

            // Assert
            Assert.That(results["sum"], Is.EqualTo(20)); // 15 + 5
            Assert.That(results["product"], Is.EqualTo(30)); // 15 * 2
            Assert.That(results.ContainsKey("__callbacks__"), Is.True);
            var callbacks = results["__callbacks__"] as List<Dictionary<string, object>>;
            Assert.That(callbacks, Is.Not.Null);
            Assert.That(callbacks.Count, Is.EqualTo(2));
            
            // Verify first callback
            Assert.That(callbacks[0]["name"], Is.EqualTo("notify"));
            Assert.That(callbacks[0]["value"], Is.EqualTo(20));
            
            // Verify second callback
            Assert.That(callbacks[1]["name"], Is.EqualTo("alert"));
            Assert.That(callbacks[1]["value"], Is.EqualTo(30));
        }

        [Test]
        public void ExecuteLuaRule_WithNestedCalculatedValuesInCallback_ReturnsExpectedResult()
        {
            // Arrange
            var ruleJson = @"{
                ""ruleId"": ""nested_calculated_callback_rule"",
                ""ruleName"": ""Nested Calculated Callback Rule"",
                ""type"": ""lua"",
                ""conditionExpression"": ""return value > 10"",
                ""actionExpressions"": {
                    ""baseValue"": ""return value * 2"",
                    ""finalValue"": ""return baseValue + 10"",
                    ""notify"": ""callback(finalValue)""
                }
            }";

            var rule = _parser.Parse(ruleJson);
            var inputs = new Dictionary<string, object>
            {
                { "value", 15 }
            };

            // Act
            var results = rule.Execute(inputs);

            // Assert
            Assert.That(results["baseValue"], Is.EqualTo(30)); // 15 * 2
            Assert.That(results["finalValue"], Is.EqualTo(40)); // 30 + 10
            Assert.That(results.ContainsKey("__callbacks__"), Is.True);
            var callbacks = results["__callbacks__"] as List<Dictionary<string, object>>;
            Assert.That(callbacks, Is.Not.Null);
            Assert.That(callbacks.Count, Is.EqualTo(1));
            Assert.That(callbacks[0]["name"], Is.EqualTo("notify"));
            Assert.That(callbacks[0]["value"], Is.EqualTo(40));
        }

        [Test]
        public void ExecuteLuaRule_WithMathAbsOnly_ReturnsExpectedResult()
        {
            // Arrange
            var ruleJson = @"{
  ""ruleId"": ""test_rule"",
  ""ruleName"": ""Test Rule"",
  ""type"": ""lua"",
  ""conditionExpression"": ""return true"",
  ""actionExpressions"": {
    ""abs"": ""return math.abs(-10)""
  }
}";

            var rule = _parser.Parse(ruleJson);
            var inputs = new Dictionary<string, object>();

            // Act
            var results = rule.Execute(inputs);

            // Assert
            Assert.That(Convert.ToDouble(results["abs"]), Is.EqualTo(10.0));
        }
    }
}
