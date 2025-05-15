using System;
using RuleEngine.Rules.Factories;

namespace RuleEngine.Core
{
    /// <summary>
    /// Builder for creating and configuring a rule engine
    /// </summary>
    public class RuleEngineBuilder
    {
        private readonly RuleFactoryRegistry _factoryRegistry;
        private readonly OperatorRegistry _operatorRegistry;
        private RuleValidator _validator;
        
        /// <summary>
        /// Creates a new rule engine builder with default configuration
        /// </summary>
        public RuleEngineBuilder()
        {
            _operatorRegistry = new OperatorRegistry();
            _factoryRegistry = new RuleFactoryRegistry();
            _validator = new RuleValidator(_operatorRegistry);
            
            // Register default factories
            RegisterDefaultFactories();
        }
        
        /// <summary>
        /// Registers the default rule factories
        /// </summary>
        private void RegisterDefaultFactories()
        {
            // Register simple rule factory
            _factoryRegistry.RegisterFactory(new SimpleRuleFactory(_operatorRegistry));
            
            // Register expression rule factory
            _factoryRegistry.RegisterFactory(new ExpressionRuleFactory());
            
            // Register composite rule factory
            _factoryRegistry.RegisterFactory(new CompositeRuleFactory(_operatorRegistry, _factoryRegistry));
        }
        
        /// <summary>
        /// Registers a custom rule factory
        /// </summary>
        /// <param name="factory">The factory to register</param>
        /// <returns>This builder for method chaining</returns>
        public RuleEngineBuilder RegisterFactory(IRuleFactory factory)
        {
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));
                
            _factoryRegistry.RegisterFactory(factory);
            return this;
        }
        
        /// <summary>
        /// Registers custom condition operators
        /// </summary>
        /// <param name="operators">The operators to register</param>
        /// <returns>This builder for method chaining</returns>
        public RuleEngineBuilder RegisterConditionOperators(params string[] operators)
        {
            _operatorRegistry.RegisterConditionOperators(operators);
            return this;
        }
        
        /// <summary>
        /// Registers custom action operators
        /// </summary>
        /// <param name="operators">The operators to register</param>
        /// <returns>This builder for method chaining</returns>
        public RuleEngineBuilder RegisterActionOperators(params string[] operators)
        {
            _operatorRegistry.RegisterActionOperators(operators);
            return this;
        }
        
        /// <summary>
        /// Builds a rule engine
        /// </summary>
        /// <returns>A configured rule engine</returns>
        public RuleEngine Build()
        {
            var parser = new JsonRuleParser(_factoryRegistry);
            return new RuleEngine(parser);
        }
        
        /// <summary>
        /// Gets the rule validator
        /// </summary>
        /// <returns>The rule validator</returns>
        public RuleValidator GetValidator()
        {
            return _validator;
        }
    }
} 