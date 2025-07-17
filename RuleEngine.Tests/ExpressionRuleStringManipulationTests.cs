using NUnit.Framework;
using RuleEngine.Core;
using RuleEngine.Rules.Factories;


namespace RuleEngine.Tests
{
    [TestFixture]
    public class ExpressionRuleStringManipulationTests
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

        // A helper method to create a standard rule JSON for action expressions
        private string CreateRuleJson(string actionExpression)
        {
            return $@"{{
                ""ruleId"": ""string_manipulation_rule"",
                ""ruleName"": ""String Manipulation Test Rule"",
                ""type"": ""expression"",
                ""conditionExpression"": ""true"",
                ""actionExpressions"": {{
                    ""result"": ""{actionExpression}""
                }}
            }}";
        }

        [TestCase("join(list(part1, ' ', part2), '')", "Hello World", "Should concatenate two strings")]
        [TestCase("contains(mainString, 'Welcome')", true, "Should find a substring")]
        [TestCase("startsWith(mainString, 'Welcome')", true, "Should check the start of a string")]
        [TestCase("endsWith(mainString, 'World!')", true, "Should check the end of a string")]
        [TestCase("indexOf('banana', 'na')", 2, "Should find the first index of a substring")]
        [TestCase("lastIndexOf('banana', 'na')", 4, "Should find the last index of a substring")]
        [TestCase("length(mainString)", 14, "Should return the correct string length")]
        [TestCase("toLower('MIXED CaSe')", "mixed case", "Should convert string to lower case")]
        [TestCase("toUpper('mixed case')", "MIXED CASE", "Should convert string to upper case")]
        [TestCase("trim('  padded text  ')", "padded text", "Should trim whitespace from both ends")]
        [TestCase("substring(mainString, 8, 5)", "World", "Should extract a substring")]
        [TestCase("replace('cat in the hat', 'cat', 'dog')", "dog in the hat", "Should replace a substring")]
        [TestCase("padLeft('42', 5, '0')", "00042", "Should pad the left of a string")]
        public void ExecuteExpressionRule_WithStringManipulationFunctions_ReturnsExpectedResult(
            string actionExpression, object expectedResult, string message)
        {
            // Arrange
            var ruleJson = CreateRuleJson(actionExpression);
            var rule = _parser.Parse(ruleJson);
            var inputs = new Dictionary<string, object>
            {
                { "mainString", "Welcome World!" },
                { "part1", "Hello" },
                { "part2", "World" }
            };

            // Act
            var results = rule.Execute(inputs);

            // Assert
            Assert.That(results.ContainsKey("result"), Is.True);
            Assert.That(results["result"], Is.EqualTo(expectedResult), message);
        }

        [Test]
        public void ExecuteExpressionRule_WithComplexChainedStringOperations_ReturnsExpectedResult()
        {
            // Arrange
            // Operation: take a substring, make it uppercase, then concatenate it with another string.
            var actionExpression = "join(list(toUpper(substring(text, 6, 5)), ' IS THE BEST PART'), '')";
            var ruleJson = CreateRuleJson(actionExpression);

            var rule = _parser.Parse(ruleJson);
            var inputs = new Dictionary<string, object>
            {
                { "text", "Hello world, this is a test." }
            };

            // Act
            var results = rule.Execute(inputs);

            // Assert
            Assert.That(results.ContainsKey("result"), Is.True);
            Assert.That(results["result"], Is.EqualTo("WORLD IS THE BEST PART"));
        }
    }
}