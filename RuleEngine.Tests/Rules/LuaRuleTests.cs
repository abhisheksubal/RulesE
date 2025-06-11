using System;
using System.Collections.Generic;
using NUnit.Framework;
using RuleEngine.Core;
using RuleEngine.Rules;

namespace RuleEngine.Tests.Rules
{
    [TestFixture]
    public class LuaRuleTests
    {
        private LuaRule _rule;
        private Dictionary<string, string> _actionScripts;

        [SetUp]
        public void Setup()
        {
            _actionScripts = new Dictionary<string, string>
            {
                { "result", "return input1 + input2" }
            };
        }

        [Test]
        public void Constructor_WithValidParameters_CreatesInstance()
        {
            // Arrange & Act
            _rule = new LuaRule("test-rule", "Test Rule", "return true", _actionScripts);

            // Assert
            Assert.That(_rule, Is.Not.Null);
            Assert.That(_rule.RuleId, Is.EqualTo("test-rule"));
            Assert.That(_rule.RuleName, Is.EqualTo("Test Rule"));
        }

        [Test]
        public void Constructor_WithNullConditionScript_ThrowsArgumentNullException()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentNullException>(() => new LuaRule("test-rule", "Test Rule", null, _actionScripts));
        }

        [Test]
        public void Constructor_WithNullActionScripts_ThrowsArgumentNullException()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentNullException>(() => new LuaRule("test-rule", "Test Rule", "return true", null));
        }

        [Test]
        public void Evaluate_WithTrueCondition_ReturnsTrue()
        {
            // Arrange
            _rule = new LuaRule("test-rule", "Test Rule", "return true", _actionScripts);
            var inputs = new Dictionary<string, object>();

            // Act
            var result = _rule.Evaluate(inputs);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void Evaluate_WithFalseCondition_ReturnsFalse()
        {
            // Arrange
            _rule = new LuaRule("test-rule", "Test Rule", "return false", _actionScripts);
            var inputs = new Dictionary<string, object>();

            // Act
            var result = _rule.Evaluate(inputs);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void Evaluate_WithInputValues_EvaluatesCorrectly()
        {
            // Arrange
            _rule = new LuaRule("test-rule", "Test Rule", "return input1 > input2", _actionScripts);
            var inputs = new Dictionary<string, object>
            {
                { "input1", 10 },
                { "input2", 5 }
            };

            // Act
            var result = _rule.Evaluate(inputs);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void Execute_WhenConditionIsFalse_ReturnsEmptyResults()
        {
            // Arrange
            _rule = new LuaRule("test-rule", "Test Rule", "return false", _actionScripts);
            var inputs = new Dictionary<string, object>();

            // Act
            var results = _rule.Execute(inputs);

            // Assert
            Assert.That(results, Is.Empty);
        }

        [Test]
        public void Execute_WhenConditionIsTrue_ExecutesActions()
        {
            // Arrange
            _rule = new LuaRule("test-rule", "Test Rule", "return true", _actionScripts);
            var inputs = new Dictionary<string, object>
            {
                { "input1", 10 },
                { "input2", 5 }
            };

            // Act
            var results = _rule.Execute(inputs);

            // Assert
            Assert.That(results, Contains.Key("result"));
            Assert.That(results["result"], Is.EqualTo(15));
        }

        [Test]
        public void Execute_WithMultipleActions_ExecutesAllActions()
        {
            // Arrange
            var multipleActions = new Dictionary<string, string>
            {
                { "sum", "return input1 + input2" },
                { "difference", "return input1 - input2" }
            };
            _rule = new LuaRule("test-rule", "Test Rule", "return true", multipleActions);
            var inputs = new Dictionary<string, object>
            {
                { "input1", 10 },
                { "input2", 5 }
            };

            // Act
            var results = _rule.Execute(inputs);

            // Assert
            Assert.That(results, Contains.Key("sum"));
            Assert.That(results, Contains.Key("difference"));
            Assert.That(results["sum"], Is.EqualTo(15));
            Assert.That(results["difference"], Is.EqualTo(5));
        }

        [Test]
        public void Execute_WithCallbackAction_AddsCallbackToResults()
        {
            // Arrange
            var actionsWithCallback = new Dictionary<string, string>
            {
                { "result", "return input1 + input2" },
                { "callback", "=>result" }
            };
            _rule = new LuaRule("test-rule", "Test Rule", "return true", actionsWithCallback);
            var inputs = new Dictionary<string, object>
            {
                { "input1", 10 },
                { "input2", 5 }
            };

            // Act
            var results = _rule.Execute(inputs);

            // Assert
            Assert.That(results, Contains.Key("__callbacks__"));
            var callbacks = (List<Dictionary<string, object>>)results["__callbacks__"];
            Assert.That(callbacks, Has.Count.EqualTo(1));
            Assert.That(callbacks[0]["name"], Is.EqualTo("callback"));
            Assert.That(callbacks[0]["value"], Is.EqualTo(15));
        }

        [Test]
        public void Execute_WithInvalidLuaScript_ThrowsInvalidOperationException()
        {
            // Arrange
            _rule = new LuaRule("test-rule", "Test Rule", "return true", new Dictionary<string, string>
            {
                { "result", "invalid lua code" }
            });
            var inputs = new Dictionary<string, object>();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => _rule.Execute(inputs));
        }
    }
} 