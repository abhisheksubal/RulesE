using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using NUnit.Framework;
using RuleEngine.Core;
using RuleEngine.Rules;
using RuleEngine.Rules.Factories;
using System.Text.Json.Serialization;

namespace RuleEngine.Tests
{
    [TestFixture]
    public class RuleExecutionSequenceTests
    {
        private RuleEngine.Core.RuleEngine _ruleEngine;
        private JsonRuleParser _ruleParser;

        [OneTimeSetUp]
        public void Setup()
        {
            // Initialize rule parser with necessary components
            var operatorRegistry = new OperatorRegistry();
            var factoryRegistry = new RuleFactoryRegistry();
            
            // Register factories
            factoryRegistry.RegisterFactory(new ExpressionRuleFactory());
            factoryRegistry.RegisterFactory(new SimpleRuleFactory(operatorRegistry));
            factoryRegistry.RegisterFactory(new CompositeRuleFactory(operatorRegistry, factoryRegistry));

            _ruleParser = new JsonRuleParser(factoryRegistry);
            _ruleEngine = new RuleEngine.Core.RuleEngine(_ruleParser);

            // Read and add rules from JSON file using original property names
            var rulesJsonPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "Rules", "expression_rules.json");
            var rulesJson = File.ReadAllText(rulesJsonPath);
            using var doc = JsonDocument.Parse(rulesJson);
            foreach (var element in doc.RootElement.GetProperty("rules").EnumerateArray())
            {
                _ruleEngine.AddRule(element.GetRawText());
            }
        }

        [Test]
        public void TestRuleExecutionSequence_AdultVotingEligibility()
        {
            // Arrange
            var inputs = new Dictionary<string, object>
            {
                { "age", 20 }
            };

            // Act
            var results = _ruleEngine.ExecuteRules(inputs);

            // Assert
            Assert.That(results, Is.Not.Null, "Results should not be null");
            Assert.That(results.ContainsKey("status"), Is.True, "Results should contain 'status'");
            Assert.That(results["status"], Is.EqualTo("adult"), "Status should be 'adult'");
            Assert.That(results.ContainsKey("canVote"), Is.True, "Results should contain 'canVote'");
            Assert.That(results["canVote"], Is.True, "canVote should be true");
        }

        [Test]
        public void TestRuleExecutionSequence_MinorNoVoting()
        {
            // Arrange
            var inputs = new Dictionary<string, object>
            {
                { "age", 16 }
            };

            // Act
            var results = _ruleEngine.ExecuteRules(inputs);

            // Assert
            Assert.That(results, Is.Not.Null, "Results should not be null");
            Assert.That(results.ContainsKey("status"), Is.False, "Results should not contain 'status' for minor");
            Assert.That(results.ContainsKey("canVote"), Is.False, "Results should not contain 'canVote' for minor");
        }

        [Test]
        public void TestRuleExecutionSequence_VerifyRuleOrder()
        {
            // Arrange
            var inputs = new Dictionary<string, object>
            {
                { "age", 20 }
            };

            // Act
            var results = _ruleEngine.ExecuteRules(inputs);

            // Assert
            Assert.That(results, Is.Not.Null, "Results should not be null");
            
            // Verify that the first rule's output is used by the second rule
            Assert.That(results.ContainsKey("status"), Is.True, "First rule should set status");
            Assert.That(results.ContainsKey("canVote"), Is.True, "Second rule should set canVote");
            
            // Verify the values are correct
            Assert.That(results["status"], Is.EqualTo("adult"), "First rule should set status to 'adult'");
            Assert.That(results["canVote"], Is.True, "Second rule should set canVote to true based on status");
        }

        [Test]
        public void TestRuleExecutionSequence_WithInvalidInput()
        {
            // Arrange
            var inputs = new Dictionary<string, object>
            {
                { "age", "invalid" } // Invalid age value
            };

            // Act
            var results = _ruleEngine.ExecuteRules(inputs);

            // Debug print
            TestContext.WriteLine("Results dictionary:");
            foreach (var kvp in results)
            {
                TestContext.WriteLine($"{kvp.Key}: {kvp.Value}");
            }

            // Assert
            Assert.That(results, Is.Not.Null, "Results should not be null");
            Assert.That(results.Count, Is.EqualTo(1), "Results should only contain the original input");
            Assert.That(results.ContainsKey("age"), Is.True, "Results should contain the original 'age' input");
            Assert.That(results["age"], Is.EqualTo("invalid"), "Results should contain the original invalid value");
        }

        [Test]
        public void TestRuleExecutionSequence_WithNullInput()
        {
            // Arrange
            Dictionary<string, object> inputs = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => _ruleEngine.ExecuteRules(inputs),
                "Should throw ArgumentNullException for null input");
        }
    }

    // Helper class to deserialize the rules JSON
    public class RulesDefinition
    {
        [JsonPropertyName("rules")]
        public List<RuleDefinition> Rules { get; set; }
    }

    public class RuleDefinition
    {
        public string RuleId { get; set; }
        public string RuleName { get; set; }
        public string Type { get; set; }
        public string ConditionExpression { get; set; }
        public Dictionary<string, string> ActionExpressions { get; set; }
    }
} 