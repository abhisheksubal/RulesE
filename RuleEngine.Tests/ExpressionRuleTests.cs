
using NUnit.Framework;
using RuleEngine.Core;
using RuleEngine.Rules.Factories;

namespace RuleEngine.Tests
{
    [TestFixture]
    public class ExpressionRuleTests
    {
        private Core.RuleEngine _ruleEngine;
        private JsonRuleParser _parser;

        [SetUp]
        public void Setup()
        {
            var factoryRegistry = new RuleFactoryRegistry();
            factoryRegistry.RegisterFactory(new ExpressionRuleFactory());
            _parser = new JsonRuleParser(factoryRegistry);
            _ruleEngine = new Core.RuleEngine(_parser);
        }

        [Test]
        public void CreateExpressionRule_WithValidDefinition_Success()
        {
            // Arrange
            var ruleJson = @"{
                ""ruleId"": ""test_rule"",
                ""ruleName"": ""Test Rule"",
                ""type"": ""expression"",
                ""conditionExpression"": ""value > 10"",
                ""actionExpressions"": {
                    ""result"": ""value * 2""
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
        public void CreateExpressionRule_WithInvalidDefinition_DoesNotThrowException()
        {
            // Arrange
            var ruleJson = @"{
                ""ruleId"": ""test_rule"",
                ""ruleName"": ""Test Rule"",
                ""type"": ""expression"",
                ""conditionExpression"": ""invalid expression"",
                ""actionExpressions"": {
                    ""result"": ""value * 2""
                }
            }";

            // Act & Assert
            Assert.DoesNotThrow(() => _parser.Parse(ruleJson));
        }

        [TestCase("value > 10", 15, true)]
        [TestCase("value > 10", 5, false)]
        [TestCase("value >= 10", 10, true)]
        [TestCase("value < 10", 5, true)]
        [TestCase("value <= 10", 10, true)]
        [TestCase("value == 10", 10, true)]
        [TestCase("value != 10", 5, true)]
        public void EvaluateExpressionRule_WithComparisonOperators_ReturnsExpectedResult(
            string conditionExpression, int value, bool expectedResult)
        {
            // Arrange
            var ruleJson = $@"{{
                ""ruleId"": ""test_rule"",
                ""ruleName"": ""Test Rule"",
                ""type"": ""expression"",
                ""conditionExpression"": ""{conditionExpression}"",
                ""actionExpressions"": {{
                    ""result"": ""value * 2""
                }}
            }}";

            var rule = _parser.Parse(ruleJson);
            var inputs = new Dictionary<string, object> { { "value", value } };

            // Act
            var result = rule.Evaluate(inputs);

            // Assert
            Assert.That(result, Is.EqualTo(expectedResult));
        }

        [TestCase("value1 > 10 && value2 < 20", 15, 15, true)]
        [TestCase("value1 > 10 && value2 < 20", 5, 15, false)]
        [TestCase("value1 > 10 || value2 < 20", 5, 15, true)]
        [TestCase("!(value1 > 10)", 15, 0, false)]
        public void EvaluateExpressionRule_WithLogicalOperators_ReturnsExpectedResult(
            string conditionExpression, int value1, int value2, bool expectedResult)
        {
            // Arrange
            var ruleJson = $@"{{
                ""ruleId"": ""test_rule"",
                ""ruleName"": ""Test Rule"",
                ""type"": ""expression"",
                ""conditionExpression"": ""{conditionExpression}"",
                ""actionExpressions"": {{
                    ""result"": ""value1 + value2""
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

        [TestCase("value + 5", 10, 15.0)]
        [TestCase("value - 5", 10, 5.0)]
        [TestCase("value * 2", 10, 20.0)]
        [TestCase("value / 2", 10, 5.0)]
        [TestCase("value % 3", 10, 1.0)]
        public void ExecuteExpressionRule_WithArithmeticOperators_ReturnsExpectedResult(
            string actionExpression, int value, double expectedResult)
        {
            // Arrange
            var ruleJson = $@"{{
                ""ruleId"": ""test_rule"",
                ""ruleName"": ""Test Rule"",
                ""type"": ""expression"",
                ""conditionExpression"": ""true"",
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
        public void ExecuteExpressionRule_WithMultipleActionExpressions_ReturnsAllResults()
        {
            // Arrange
            var ruleJson = @"{
                ""ruleId"": ""test_rule"",
                ""ruleName"": ""Test Rule"",
                ""type"": ""expression"",
                ""conditionExpression"": ""true"",
                ""actionExpressions"": {
                    ""sum"": ""value1 + value2"",
                    ""product"": ""value1 * value2"",
                    ""average"": ""(value1 + value2) / 2""
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
        public void ExecuteExpressionRule_WithStringOperations_ReturnsExpectedResult()
        {
            // Arrange
            var ruleJson = @"{
                ""ruleId"": ""test_rule"",
                ""ruleName"": ""Test Rule"",
                ""type"": ""expression"",
                ""conditionExpression"": ""true"",
                ""actionExpressions"": {
                    ""concatenated"": ""firstName + ' ' + lastName"",
                    ""upperCase"": ""ToUpper(firstName)"",
                    ""lowerCase"": ""ToLower(lastName)"",
                    ""nullCheck"": ""IsNull(nullValue)"",
                    ""defaultValue"": ""Nvl(nullValue, 'default')""
                }
            }";

            var rule = _parser.Parse(ruleJson);
            var inputs = new Dictionary<string, object>
            {
                { "firstName", "John" },
                { "lastName", "Doe" },
                { "nullValue", null }
            };

            // Act
            var results = rule.Execute(inputs);

            // Assert
            Assert.That(results["concatenated"], Is.EqualTo("John Doe"));
            Assert.That(results["upperCase"], Is.EqualTo("JOHN"));
            Assert.That(results["lowerCase"], Is.EqualTo("doe"));
            Assert.That(results["nullCheck"], Is.EqualTo(true));
            Assert.That(results["defaultValue"], Is.EqualTo("default"));
        }

        [Test]
        public void ExecuteExpressionRule_WithConditionalExpressions_ReturnsExpectedResult()
        {
            // Arrange
            var ruleJson = @"{
                ""ruleId"": ""test_rule"",
                ""ruleName"": ""Test Rule"",
                ""type"": ""expression"",
                ""conditionExpression"": ""true"",
                ""actionExpressions"": {
                    ""result"": ""if(score >= 60, 'Pass', 'Fail')"",
                    ""grade"": ""if(score >= 90, 'A', if(score >= 80, 'B', if(score >= 70, 'C', if(score >= 60, 'D', 'F'))))""
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
        public void ExecuteExpressionRule_WithMathFunctions_ReturnsExpectedResult()
        {
            // Arrange
            var ruleJson = @"{
                ""ruleId"": ""test_rule"",
                ""ruleName"": ""Test Rule"",
                ""type"": ""expression"",
                ""conditionExpression"": ""true"",
                ""actionExpressions"": {
                    ""abs"": ""Abs(-10)"",
                    ""round"": ""Round(3.14159, 2)"",
                    ""max"": ""Max(10, 20)"",
                    ""min"": ""Min(10, 20)"",
                    ""sqrt"": ""Sqrt(16)""
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
        public void ExecuteExpressionRule_WithMissingInputs_ThrowsInvalidOperationException()
        {
            // Arrange
            var ruleJson = @"{
                ""ruleId"": ""test_rule"",
                ""ruleName"": ""Test Rule"",
                ""type"": ""expression"",
                ""conditionExpression"": ""value > 10"",
                ""actionExpressions"": {
                    ""result"": ""value * 2""
                }
            }";

            var rule = _parser.Parse(ruleJson);
            var inputs = new Dictionary<string, object>();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => rule.Execute(inputs));
        }

        [Test]
        public void ExecuteExpressionRule_WithInvalidExpression_ThrowsInvalidOperationException()
        {
            // Arrange
            var ruleJson = @"{
                ""ruleId"": ""test_rule"",
                ""ruleName"": ""Test Rule"",
                ""type"": ""expression"",
                ""conditionExpression"": ""true"",
                ""actionExpressions"": {
                    ""result"": ""invalid expression""
                }
            }";

            var rule = _parser.Parse(ruleJson);
            var inputs = new Dictionary<string, object>();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => rule.Execute(inputs));
        }

        [Test]
        public void ExecuteExpressionRule_WithNullValues_HandlesGracefully()
        {
            // Arrange
            var ruleJson = @"{
                ""ruleId"": ""test_rule"",
                ""ruleName"": ""Test Rule"",
                ""type"": ""expression"",
                ""conditionExpression"": ""true"",
                ""actionExpressions"": {
                    ""nullCheck"": ""IsNull(value)"",
                    ""defaultValue"": ""Nvl(value, 'default')"",
                    ""nullArithmetic"": ""Nvl(value, 0) + 5""
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

        [Test, Ignore("NCalc does not support Convert.ToDouble/ToString by default")]
        public void ExecuteExpressionRule_WithTypeConversion_HandlesCorrectly()
        {
            // Arrange
            var ruleJson = @"{
                ""ruleId"": ""test_rule"",
                ""ruleName"": ""Test Rule"",
                ""type"": ""expression"",
                ""conditionExpression"": ""true"",
                ""actionExpressions"": {
                    ""stringToNumber"": ""Convert.ToDouble('123.45')"",
                    ""numberToString"": ""Convert.ToString(123.45)"",
                    ""booleanToString"": ""Convert.ToString(true)""
                }
            }";

            var rule = _parser.Parse(ruleJson);
            var inputs = new Dictionary<string, object>();

            // Act
            var results = rule.Execute(inputs);

            // Assert
            Assert.That(Convert.ToDouble(results["stringToNumber"]), Is.EqualTo(123.45));
            Assert.That(results["numberToString"], Is.EqualTo("123.45"));
            Assert.That(results["booleanToString"], Is.EqualTo("True"));
        }
    }
} 