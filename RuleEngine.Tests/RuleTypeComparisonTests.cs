using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using NUnit.Framework;
using RuleEngine.Core;
using RuleEngine.Rules;
using RuleEngine.Rules.Factories;

namespace RuleEngine.Tests
{
    [TestFixture]
    public class RuleTypeComparisonTests
    {
        private RuleEngine.Core.RuleEngine _expressionRuleEngine;
        private RuleEngine.Core.RuleEngine _luaRuleEngine;
        private JsonRuleParser _expressionRuleParser;
        private JsonRuleParser _luaRuleParser;

        [SetUp]
        public void Setup()
        {
            // Setup for expression rules
            var expressionFactoryRegistry = new RuleFactoryRegistry();
            expressionFactoryRegistry.RegisterFactory(new ExpressionRuleFactory());
            _expressionRuleParser = new JsonRuleParser(expressionFactoryRegistry);
            _expressionRuleEngine = new RuleEngine.Core.RuleEngine(_expressionRuleParser);

            // Setup for Lua rules
            var luaFactoryRegistry = new RuleFactoryRegistry();
            luaFactoryRegistry.RegisterFactory(new LuaRuleFactory());
            _luaRuleParser = new JsonRuleParser(luaFactoryRegistry);
            _luaRuleEngine = new RuleEngine.Core.RuleEngine(_luaRuleParser);

            // Load expression rules
            var expressionRulesPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "Rules", "rules_example.json");
            var expressionRulesJson = File.ReadAllText(expressionRulesPath);
            using (var doc = JsonDocument.Parse(expressionRulesJson))
            {
                foreach (var element in doc.RootElement.GetProperty("rules").EnumerateArray())
                {
                    _expressionRuleEngine.AddRule(element.GetRawText());
                }
            }

            // Load Lua rules
            var luaRulesPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "Rules", "rules_example_lua.json");
            var luaRulesJson = File.ReadAllText(luaRulesPath);
            using (var doc = JsonDocument.Parse(luaRulesJson))
            {
                foreach (var element in doc.RootElement.GetProperty("rules").EnumerateArray())
                {
                    _luaRuleEngine.AddRule(element.GetRawText());
                }
            }
        }

        [Test]
        public void TestWOFCollectionRule_InitialState()
        {
            // Arrange
            var inputs = new Dictionary<string, object>
            {
                { "counter", "gameplay" },
                { "class_Value", "spin_gained" },
                { "family", "wheel_of_fortune" },
                { "wof_collection_rule_progress", 0 },
                { "mk_collection_rule_progress", 0 },
                { "completed_count", 0 },
                { "wof_collection_rule_completed", false },
                { "mk_collection_rule_completed", false },
                { "wof_collection_rule_target", new[] { 5, 10, 15, 20 } },
                { "mk_collection_rule_target", new[] { 5, 10, 15, 20 } },
                { "stage", 1 },
                { "task_count", 2 }
            };

            // Act
            var expressionResults = _expressionRuleEngine.ExecuteRules(inputs);
            var luaResults = _luaRuleEngine.ExecuteRules(inputs);

            // Assert
            Assert.That(luaResults["wof_collection_rule_progress"], Is.EqualTo(expressionResults["wof_collection_rule_progress"]));
            Assert.That(luaResults["completed_count"], Is.EqualTo(expressionResults["completed_count"]));
            Assert.That(luaResults["wof_collection_rule_completed"], Is.EqualTo(expressionResults["wof_collection_rule_completed"]));
        }

        [Test]
        public void TestWOFCollectionRule_ProgressCompletion()
        {
            // Arrange
            var inputs = new Dictionary<string, object>
            {
                { "counter", "gameplay" },
                { "class_Value", "spin_gained" },
                { "family", "wheel_of_fortune" },
                { "wof_collection_rule_progress", 4 },
                { "mk_collection_rule_progress", 0 },
                { "completed_count", 0 },
                { "wof_collection_rule_completed", false },
                { "mk_collection_rule_completed", false },
                { "wof_collection_rule_target", new[] { 5, 10, 15, 20 } },
                { "mk_collection_rule_target", new[] { 5, 10, 15, 20 } },
                { "stage", 1 },
                { "task_count", 2 }
            };

            // Act
            var expressionResults = _expressionRuleEngine.ExecuteRules(inputs);
            var luaResults = _luaRuleEngine.ExecuteRules(inputs);

            // Assert
            Assert.That(luaResults["wof_collection_rule_progress"], Is.EqualTo(expressionResults["wof_collection_rule_progress"]));
            Assert.That(luaResults["completed_count"], Is.EqualTo(expressionResults["completed_count"]));
            Assert.That(luaResults["wof_collection_rule_completed"], Is.EqualTo(expressionResults["wof_collection_rule_completed"]));
        }

        [Test]
        public void TestMKCollectionRule_InitialState()
        {
            // Arrange
            var inputs = new Dictionary<string, object>
            {
                { "counter", "gameplay" },
                { "class_Value", "spin_gained" },
                { "family", "wheel_of_fortune" },
                { "wof_collection_rule_progress", 0 },
                { "mk_collection_rule_progress", 0 },
                { "completed_count", 0 },
                { "wof_collection_rule_completed", false },
                { "mk_collection_rule_completed", false },
                { "wof_collection_rule_target", new[] { 5, 10, 15, 20 } },
                { "mk_collection_rule_target", new[] { 5, 10, 15, 20 } },
                { "stage", 1 },
                { "task_count", 2 }
            };

            // Act
            var expressionResults = _expressionRuleEngine.ExecuteRules(inputs);
            var luaResults = _luaRuleEngine.ExecuteRules(inputs);

            // Assert
            Assert.That(luaResults["mk_collection_rule_progress"], Is.EqualTo(expressionResults["mk_collection_rule_progress"]));
            Assert.That(luaResults["completed_count"], Is.EqualTo(expressionResults["completed_count"]));
            Assert.That(luaResults["mk_collection_rule_completed"], Is.EqualTo(expressionResults["mk_collection_rule_completed"]));
        }

        [Test]
        public void TestStageClearRule_StageProgression()
        {
            // Arrange
            var inputs = new Dictionary<string, object>
            {
                { "counter", "gameplay" },
                { "class_Value", "spin_gained" },
                { "family", "wheel_of_fortune" },
                { "wof_collection_rule_progress", 0 },
                { "mk_collection_rule_progress", 0 },
                { "completed_count", 2 },
                { "wof_collection_rule_completed", false },
                { "mk_collection_rule_completed", false },
                { "wof_collection_rule_target", new[] { 5, 10, 15, 20 } },
                { "mk_collection_rule_target", new[] { 5, 10, 15, 20 } },
                { "stage", 1 },
                { "task_count", 2 }
            };

            // Act
            var expressionResults = _expressionRuleEngine.ExecuteRules(inputs);
            var luaResults = _luaRuleEngine.ExecuteRules(inputs);

            // Assert
            Assert.That(luaResults["completed_count"], Is.EqualTo(expressionResults["completed_count"]), "Completed count should match between expression and Lua rules");
            Assert.That(luaResults["stage"], Is.EqualTo(expressionResults["stage"]), "Stage should match between expression and Lua rules");
            Assert.That(luaResults["wof_collection_rule_progress"], Is.EqualTo(expressionResults["wof_collection_rule_progress"]), "WOF progress should match between expression and Lua rules");
            Assert.That(luaResults["mk_collection_rule_progress"], Is.EqualTo(expressionResults["mk_collection_rule_progress"]), "MK progress should match between expression and Lua rules");
            Assert.That(luaResults["wof_collection_rule_completed"], Is.EqualTo(expressionResults["wof_collection_rule_completed"]), "WOF completion should match between expression and Lua rules");
            Assert.That(luaResults["mk_collection_rule_completed"], Is.EqualTo(expressionResults["mk_collection_rule_completed"]), "MK completion should match between expression and Lua rules");
        }

        [Test]
        public void TestStageClearRule_MaxStage()
        {
            // Arrange
            var inputs = new Dictionary<string, object>
            {
                { "counter", "gameplay" },
                { "class_Value", "spin_gained" },
                { "family", "wheel_of_fortune" },
                { "wof_collection_rule_progress", 0 },
                { "mk_collection_rule_progress", 0 },
                { "completed_count", 2 },
                { "wof_collection_rule_completed", false },
                { "mk_collection_rule_completed", false },
                { "wof_collection_rule_target", new[] { 5, 10, 15, 20 } },
                { "mk_collection_rule_target", new[] { 5, 10, 15, 20 } },
                { "stage", 4 },
                { "task_count", 2 }
            };

            // Act
            var expressionResults = _expressionRuleEngine.ExecuteRules(inputs);
            var luaResults = _luaRuleEngine.ExecuteRules(inputs);

            // Assert
            Assert.That(luaResults["completed_count"], Is.EqualTo(expressionResults["completed_count"]));
            Assert.That(luaResults["stage"], Is.EqualTo(expressionResults["stage"]));
        }

        [Test]
        public void TestAllRules_ComplexScenario()
        {
            // Arrange
            var inputs = new Dictionary<string, object>
            {
                { "counter", "gameplay" },
                { "class_Value", "spin_gained" },
                { "family", "wheel_of_fortune" },
                { "wof_collection_rule_progress", 4 },
                { "mk_collection_rule_progress", 4 },
                { "completed_count", 1 },
                { "wof_collection_rule_completed", false },
                { "mk_collection_rule_completed", false },
                { "wof_collection_rule_target", new[] { 5, 10, 15, 20 } },
                { "mk_collection_rule_target", new[] { 5, 10, 15, 20 } },
                { "stage", 1 },
                { "task_count", 2 }
            };

            // Act
            var expressionResults = _expressionRuleEngine.ExecuteRules(inputs);
            var luaResults = _luaRuleEngine.ExecuteRules(inputs);

            // Assert
            Assert.That(luaResults["wof_collection_rule_progress"], Is.EqualTo(expressionResults["wof_collection_rule_progress"]));
            Assert.That(luaResults["mk_collection_rule_progress"], Is.EqualTo(expressionResults["mk_collection_rule_progress"]));
            Assert.That(luaResults["completed_count"], Is.EqualTo(expressionResults["completed_count"]));
            Assert.That(luaResults["wof_collection_rule_completed"], Is.EqualTo(expressionResults["wof_collection_rule_completed"]));
            Assert.That(luaResults["mk_collection_rule_completed"], Is.EqualTo(expressionResults["mk_collection_rule_completed"]));
            Assert.That(luaResults["stage"], Is.EqualTo(expressionResults["stage"]));
        }
    }
} 