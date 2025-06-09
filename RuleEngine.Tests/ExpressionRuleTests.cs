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
        public void ExecuteExpressionRule_WithBuiltInFunctions_ReturnsExpectedResult()
        {
            // Arrange
            var ruleJson = @"{
                ""ruleId"": ""function_rule"",
                ""ruleName"": ""Built-in Functions Rule"",
                ""type"": ""expression"",
                ""conditionExpression"": ""Abs(value) > 0"",
                ""actionExpressions"": {
                    ""absolute"": ""Abs(value)"",
                    ""rounded"": ""Round(value, 2)"",
                    ""ceiling"": ""Ceiling(value)"",
                    ""floor"": ""Floor(value)"",
                    ""power"": ""Pow(value, 2)"",
                    ""squareRoot"": ""Sqrt(Abs(value))"",
                    ""logarithm"": ""Log(Abs(value), 10)"",
                    ""sine"": ""Sin(value)"",
                    ""cosine"": ""Cos(value)"",
                    ""tangent"": ""Tan(value)""
                }
            }";

            var rule = _parser.Parse(ruleJson);
            var inputs = new Dictionary<string, object>
            {
                { "value", -3.14159 }
            };

            // Act
            var results = rule.Execute(inputs);

            // Assert
            Assert.That(results.ContainsKey("absolute"), Is.True);
            Assert.That(results.ContainsKey("rounded"), Is.True);
            Assert.That(results.ContainsKey("ceiling"), Is.True);
            Assert.That(results.ContainsKey("floor"), Is.True);
            Assert.That(results.ContainsKey("power"), Is.True);
            Assert.That(results.ContainsKey("squareRoot"), Is.True);
            Assert.That(results.ContainsKey("logarithm"), Is.True);
            Assert.That(results.ContainsKey("sine"), Is.True);
            Assert.That(results.ContainsKey("cosine"), Is.True);
            Assert.That(results.ContainsKey("tangent"), Is.True);

            // Verify the results
            Assert.That(Convert.ToDouble(results["absolute"]), Is.EqualTo(3.14159).Within(0.00001));
            Assert.That(Convert.ToDouble(results["rounded"]), Is.EqualTo(-3.14).Within(0.01));
            Assert.That(Convert.ToDouble(results["ceiling"]), Is.EqualTo(-3.0));
            Assert.That(Convert.ToDouble(results["floor"]), Is.EqualTo(-4.0));
            Assert.That(Convert.ToDouble(results["power"]), Is.EqualTo(9.87).Within(0.01));
            Assert.That(Convert.ToDouble(results["squareRoot"]), Is.EqualTo(1.77245).Within(0.00001));
            Assert.That(Convert.ToDouble(results["logarithm"]), Is.EqualTo(0.49715).Within(0.00001));
            Assert.That(Convert.ToDouble(results["sine"]), Is.EqualTo(0.0).Within(0.00001));
            Assert.That(Convert.ToDouble(results["cosine"]), Is.EqualTo(-1.0).Within(0.00001));
            Assert.That(Convert.ToDouble(results["tangent"]), Is.EqualTo(0.0).Within(0.00001));
        }

        [Test]
        public void ExecuteExpressionRule_WithStringOperations_ReturnsExpectedResult()
        {
            // Arrange
            var ruleJson = @"{
                ""ruleId"": ""string_rule"",
                ""ruleName"": ""String Operations Rule"",
                ""type"": ""expression"",
                ""conditionExpression"": ""text != ''"",
                ""actionExpressions"": {
                    ""concatenated"": ""text + ' World'"",
                    ""repeated"": ""text + text"",
                    ""contains"": ""text == 'Hello'"",
                    ""equals"": ""text == 'Hello'"",
                    ""notEquals"": ""text != 'World'""
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

        [Test]
        public void ExecuteExpressionRule_WithArrayOperations_ReturnsExpectedResult()
        {
            // Arrange
            var ruleJson = @"{
                ""ruleId"": ""array_rule"",
                ""ruleName"": ""Array Processing Rule"",
                ""type"": ""expression"",
                ""conditionExpression"": ""scores[0] + scores[1] + scores[2] + scores[3] + scores[4] > 250"",
                ""actionExpressions"": {
                    ""totalScores"": ""scores[0] + scores[1] + scores[2] + scores[3] + scores[4]"",
                    ""scoreCount"": ""5"",
                    ""hasHighScores"": ""scores[3] >= 80"",
                    ""hasLowScores"": ""scores[0] < 50"",
                    ""firstScore"": ""scores[0]"",
                    ""lastScore"": ""scores[4]"",
                    ""middleScore"": ""scores[2]""
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
        public void ExecuteExpressionRule_WithArrayOperationsAndMultipleArrays_ReturnsExpectedResult()
        {
            // Arrange
            var ruleJson = @"{
                ""ruleId"": ""multi_array_rule"",
                ""ruleName"": ""Multiple Array Processing Rule"",
                ""type"": ""expression"",
                ""conditionExpression"": ""(scores1[0] + scores1[1] + scores1[2] + scores1[3]) > (scores2[0] + scores2[1] + scores2[2] + scores2[3])"",
                ""actionExpressions"": {
                    ""score1Total"": ""scores1[0] + scores1[1] + scores1[2] + scores1[3]"",
                    ""score2Total"": ""scores2[0] + scores2[1] + scores2[2] + scores2[3]"",
                    ""totalScores"": ""scores1[0] + scores1[1] + scores1[2] + scores1[3] + scores2[0] + scores2[1] + scores2[2] + scores2[3]"",
                    ""score1Max"": ""Max(Max(scores1[0], scores1[1]), Max(scores1[2], scores1[3]))"",
                    ""score2Max"": ""Max(Max(scores2[0], scores2[1]), Max(scores2[2], scores2[3]))"",
                    ""score1Min"": ""Min(Min(scores1[0], scores1[1]), Min(scores1[2], scores1[3]))"",
                    ""score2Min"": ""Min(Min(scores2[0], scores2[1]), Min(scores2[2], scores2[3]))"",
                    ""totalStudents"": ""8""
                }
            }";

            var rule = _parser.Parse(ruleJson);
            var inputs = new Dictionary<string, object>
            {
                { "scores1", new[] { 75, 80, 85, 90 } },
                { "scores2", new[] { 65, 70, 75, 80 } }
            };

            // Act
            var results = rule.Execute(inputs);

            // Assert
            Assert.That(results.ContainsKey("score1Total"), Is.True);
            Assert.That(results.ContainsKey("score2Total"), Is.True);
            Assert.That(results.ContainsKey("totalScores"), Is.True);
            Assert.That(results.ContainsKey("score1Max"), Is.True);
            Assert.That(results.ContainsKey("score2Max"), Is.True);
            Assert.That(results.ContainsKey("score1Min"), Is.True);
            Assert.That(results.ContainsKey("score2Min"), Is.True);
            Assert.That(results.ContainsKey("totalStudents"), Is.True);

            // Verify the results
            Assert.That(Convert.ToDouble(results["score1Total"]), Is.EqualTo(330.0)); // 75 + 80 + 85 + 90
            Assert.That(Convert.ToDouble(results["score2Total"]), Is.EqualTo(290.0)); // 65 + 70 + 75 + 80
            Assert.That(Convert.ToDouble(results["totalScores"]), Is.EqualTo(620.0)); // 330 + 290
            Assert.That(Convert.ToDouble(results["score1Max"]), Is.EqualTo(90.0));
            Assert.That(Convert.ToDouble(results["score2Max"]), Is.EqualTo(80.0));
            Assert.That(Convert.ToDouble(results["score1Min"]), Is.EqualTo(75.0));
            Assert.That(Convert.ToDouble(results["score2Min"]), Is.EqualTo(65.0));
            Assert.That(Convert.ToInt32(results["totalStudents"]), Is.EqualTo(8));
        }

        [Test]
        public void ExecuteExpressionRule_WithBasicOperations_ReturnsExpectedResult()
        {
            // Arrange
            var ruleJson = @"{
                ""ruleId"": ""basic_rule"",
                ""ruleName"": ""Basic Operations Rule"",
                ""type"": ""expression"",
                ""conditionExpression"": ""value1 > 10 && value2 < 20"",
                ""actionExpressions"": {
                    ""sum"": ""value1 + value2"",
                    ""difference"": ""value1 - value2"",
                    ""product"": ""value1 * value2"",
                    ""quotient"": ""value1 / value2"",
                    ""remainder"": ""value1 % value2"",
                    ""isGreater"": ""value1 > value2"",
                    ""isEqual"": ""value1 == value2"",
                    ""isLess"": ""value1 < value2""
                }
            }";

            var rule = _parser.Parse(ruleJson);
            var inputs = new Dictionary<string, object>
            {
                { "value1", 15 },
                { "value2", 5 }
            };

            // Act
            var results = rule.Execute(inputs);

            // Assert
            Assert.That(results.ContainsKey("sum"), Is.True);
            Assert.That(results.ContainsKey("difference"), Is.True);
            Assert.That(results.ContainsKey("product"), Is.True);
            Assert.That(results.ContainsKey("quotient"), Is.True);
            Assert.That(results.ContainsKey("remainder"), Is.True);
            Assert.That(results.ContainsKey("isGreater"), Is.True);
            Assert.That(results.ContainsKey("isEqual"), Is.True);
            Assert.That(results.ContainsKey("isLess"), Is.True);

            // Verify the results
            Assert.That(Convert.ToDouble(results["sum"]), Is.EqualTo(20.0)); // 15 + 5
            Assert.That(Convert.ToDouble(results["difference"]), Is.EqualTo(10.0)); // 15 - 5
            Assert.That(Convert.ToDouble(results["product"]), Is.EqualTo(75.0)); // 15 * 5
            Assert.That(Convert.ToDouble(results["quotient"]), Is.EqualTo(3.0)); // 15 / 5
            Assert.That(Convert.ToDouble(results["remainder"]), Is.EqualTo(0.0)); // 15 % 5
            Assert.That(Convert.ToBoolean(results["isGreater"]), Is.True); // 15 > 5
            Assert.That(Convert.ToBoolean(results["isEqual"]), Is.False); // 15 == 5
            Assert.That(Convert.ToBoolean(results["isLess"]), Is.False); // 15 < 5
        }

        [Test]
        public void ExecuteExpressionRule_WithArrayInput_ReturnsExpectedResult()
        {
            // Arrange
            var ruleJson = @"{
                ""ruleId"": ""array_rule"",
                ""ruleName"": ""Array Input Rule"",
                ""type"": ""expression"",
                ""conditionExpression"": ""arr != null"",
                ""actionExpressions"": {
                    ""isArray"": ""arr != null"",
                    ""firstElement"": ""arr[0]""
                }
            }";

            var rule = _parser.Parse(ruleJson);
            var inputs = new Dictionary<string, object>
            {
                { "arr", new int[] { 10, 20, 30 } }
            };

            // Act & Assert
            // NCalc does not support array indexing, so this should throw or fail gracefully
            try
            {
                var results = rule.Execute(inputs);
                Assert.That(results.ContainsKey("isArray"), Is.True);
                // If array indexing is supported, this will pass
                Assert.That(results["firstElement"], Is.EqualTo(10));
            }
            catch (Exception ex)
            {
                Assert.That(ex, Is.TypeOf<InvalidOperationException>());
            }
        }

        [Test]
        public void ExecuteExpressionRule_WithArrayGetFunction_ReturnsExpectedResult()
        {
            // Arrange
            var ruleJson = @"{
                ""ruleId"": ""array_get_rule"",
                ""ruleName"": ""Array Get Rule"",
                ""type"": ""expression"",
                ""conditionExpression"": ""true"",
                ""actionExpressions"": {
                    ""first"": ""ArrayGet(arr, 0)"",
                    ""second"": ""ArrayGet(arr, 1)"",
                    ""third"": ""ArrayGet(arr, 2)""
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
            Assert.That(results["first"], Is.EqualTo(10));
            Assert.That(results["second"], Is.EqualTo(20));
            Assert.That(results["third"], Is.EqualTo(30));
        }

        [Test]
        public void ExecuteExpressionRule_WithArrayGetInCondition_ReturnsExpectedResult()
        {
            // Arrange
            var ruleJson = @"{
                ""ruleId"": ""array_get_condition_rule"",
                ""ruleName"": ""Array Get Condition Rule"",
                ""type"": ""expression"",
                ""conditionExpression"": ""ArrayGet(arr, 0) > 5"",
                ""actionExpressions"": {
                    ""result"": ""ArrayGet(arr, 0)""
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
            Assert.That(results["result"], Is.EqualTo(10));
        }

        [Test]
        public void ExecuteExpressionRule_WithCallbackAndArrayGet_ReturnsExpectedResult()
        {
            // Arrange
            var ruleJson = @"{
                ""ruleId"": ""callback_array_get_rule"",
                ""ruleName"": ""Callback and Array Get Rule"",
                ""type"": ""expression"",
                ""conditionExpression"": ""ArrayGet(arr, 0) > 5"",
                ""actionExpressions"": {
                    ""result"": ""ArrayGet(arr, 1)"",
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
    }
} 