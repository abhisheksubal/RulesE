using NUnit.Framework;
using RuleEngine.Core;
using RuleEngine.Rules.Factories;
using System.Collections.Generic;
using System;

namespace RuleEngine.Tests
{
    [TestFixture]
    public class StringManipulationRuleTests
    {
        private Core.RuleEngine ruleEngine;
        private JsonRuleParser parser;

        [SetUp]
        public void Setup()
        {
            var factoryRegistry = new RuleFactoryRegistry();
            factoryRegistry.RegisterFactory(new ExpressionRuleFactory());
            parser = new JsonRuleParser(factoryRegistry);
            ruleEngine = new Core.RuleEngine(parser);
        }

        private void AddAndExecuteRule(string actionExpression, Dictionary<string, object> inputs, out IDictionary<string, object> results)
        {
            var ruleJson = $@"{{
                ""ruleId"": ""string_manipulation_test"",
                ""ruleName"": ""String Manipulation Test"",
                ""type"": ""expression"",
                ""conditionExpression"": ""true"",
                ""actionExpressions"": {{
                    ""result"": ""{actionExpression}""
                }}
            }}";
            ruleEngine.AddRule(ruleJson);
            results = ruleEngine.ExecuteRules(inputs);
        }

        [TestCase("'HELLO WORLD'", "hello world")]
        [TestCase("'Another Test'", "another test")]
        public void TestToLowerFunction(string input, string expected)
        {
            var inputs = new Dictionary<string, object>();
            AddAndExecuteRule($"toLower({input})", inputs, out var results);
            Assert.AreEqual(expected, results["result"]);
        }

        [TestCase("'hello world'", "HELLO WORLD")]
        public void TestToUpperFunction(string input, string expected)
        {
            var inputs = new Dictionary<string, object>();
            AddAndExecuteRule($"toUpper({input})", inputs, out var results);
            Assert.AreEqual(expected, results["result"]);
        }

        [TestCase("'helloworld'", 5, 5, "world")]
        [TestCase("'testing'", 0, 4, "test")]
        public void TestSubstringFunction(string text, int startIndex, int length, string expected)
        {
            var inputs = new Dictionary<string, object>();
            AddAndExecuteRule($"substring({text}, {startIndex}, {length})", inputs, out var results);
            Assert.AreEqual(expected, results["result"]);
        }

        [TestCase("'  padded string  '", "padded string")]
        public void TestTrimFunction(string input, string expected)
        {
            var inputs = new Dictionary<string, object>();
            AddAndExecuteRule($"trim({input})", inputs, out var results);
            Assert.AreEqual(expected, results["result"]);
        }

        [TestCase("'hello'", "'o'", true)]
        [TestCase("'world'", "'z'", false)]
        public void TestContainsFunction(string haystack, string needle, bool expected)
        {
            var inputs = new Dictionary<string, object>();
            AddAndExecuteRule($"contains({haystack}, {needle})", inputs, out var results);
            Assert.AreEqual(expected, results["result"]);
        }

        [TestCase("'abcdef'", "'abc'", true)]
        [TestCase("'abcdef'", "'def'", false)]
        public void TestStartsWithFunction(string text, string prefix, bool expected)
        {
            var inputs = new Dictionary<string, object>();
            AddAndExecuteRule($"startsWith({text}, {prefix})", inputs, out var results);
            Assert.AreEqual(expected, results["result"]);
        }

        [TestCase("'abcdef'", "'def'", true)]
        [TestCase("'abcdef'", "'abc'", false)]
        public void TestEndsWithFunction(string text, string suffix, bool expected)
        {
            var inputs = new Dictionary<string, object>();
            AddAndExecuteRule($"endsWith({text}, {suffix})", inputs, out var results);
            Assert.AreEqual(expected, results["result"]);
        }

        [TestCase("'hello-world'", "'-'", "' '", "hello world")]
        public void TestReplaceFunction(string original, string oldValue, string newValue, string expected)
        {
            var inputs = new Dictionary<string, object>();
            AddAndExecuteRule($"replace({original}, {oldValue}, {newValue})", inputs, out var results);
            Assert.AreEqual(expected, results["result"]);
        }

        [Test]
        public void TestJoinFunction()
        {
            var inputs = new Dictionary<string, object>
            {
                { "items", new[] { "apple", "banana", "cherry" } }
            };
            AddAndExecuteRule("join(items, ', ')", inputs, out var results);
            Assert.AreEqual("apple, banana, cherry", results["result"]);
        }

        [Test]
        public void TestSplitFunction()
        {
            var inputs = new Dictionary<string, object>();
            AddAndExecuteRule("split('a|b|c', '|')", inputs, out var results);
            var resultList = results["result"] as List<object>;
            Assert.IsNotNull(resultList);
            CollectionAssert.AreEqual(new List<object> { "a", "b", "c" }, resultList);
        }

        [TestCase("'capitalize me'", "Capitalize me")]
        public void TestCapitalizeFunction(string input, string expected)
        {
            var inputs = new Dictionary<string, object>();
            AddAndExecuteRule($"capitalize({input})", inputs, out var results);
            Assert.AreEqual(expected, results["result"]);
        }

        [TestCase("'test'", 4)]
        public void TestLengthFunction(string input, int expected)
        {
            var inputs = new Dictionary<string, object>();
            AddAndExecuteRule($"length({input})", inputs, out var results);
            Assert.AreEqual(expected, results["result"]);
        }

        [TestCase("'abcde'", "'c'", 2)]
        public void TestIndexOfFunction(string text, string value, int expected)
        {
            var inputs = new Dictionary<string, object>();
            AddAndExecuteRule($"indexOf({text}, {value})", inputs, out var results);
            Assert.AreEqual(expected, results["result"]);
        }

        [TestCase("'abacaba'", "'b'", 5)]
        public void TestLastIndexOfFunction(string text, string value, int expected)
        {
            var inputs = new Dictionary<string, object>();
            AddAndExecuteRule($"lastIndexOf({text}, {value})", inputs, out var results);
            Assert.AreEqual(expected, results["result"]);
        }
    }
}