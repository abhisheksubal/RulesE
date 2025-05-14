using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using RuleEngine.Core;

namespace Examples.UnityIntegration
{
    /// <summary>
    /// Tracks game progress using the rule engine to evaluate achievements and milestones
    /// </summary>
    public class GameProgressTracker : MonoBehaviour
    {
        [SerializeField] private TextAsset rulesFile;
        
        private RuleEngine.Core.RuleEngine _ruleEngine;
        private GameState _gameState;
        private RewardHandler _rewardHandler;
        
        // Singleton instance
        private static GameProgressTracker _instance;
        public static GameProgressTracker Instance => _instance;

        private void Awake()
        {
            // Singleton pattern
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            _instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Initialize components
            InitializeRuleEngine();
            _gameState = new GameState();
            _rewardHandler = new RewardHandler();
            
            // Hook into the analytics tracking system
            AnalyticsManager.OnTrackEvent += HandleTrackEvent;
            
            Debug.Log("Game Progress Tracker initialized with " + _ruleEngine.GetRules().Count + " rules");
        }

        private void OnDestroy()
        {
            // Remove hook when destroyed
            AnalyticsManager.OnTrackEvent -= HandleTrackEvent;
        }

        private void InitializeRuleEngine()
        {
            var ruleParser = new JsonRuleParser();
            _ruleEngine = new RuleEngine.Core.RuleEngine(ruleParser);
            
            // Load rules from the single JSON file
            if (rulesFile != null)
            {
                try
                {
                    // Parse the JSON array of rules
                    var rules = JsonConvert.DeserializeObject<List<object>>(rulesFile.text);
                    
                    // Add each rule to the rule engine
                    foreach (var rule in rules)
                    {
                        string ruleJson = JsonConvert.SerializeObject(rule);
                        _ruleEngine.AddRule(ruleJson);
                        Debug.Log($"Loaded rule from rules file");
                    }
                    
                    Debug.Log($"Successfully loaded {rules.Count} rules from {rulesFile.name}");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Failed to load rules from {rulesFile.name}: {ex.Message}");
                }
            }
            else
            {
                Debug.LogWarning("No rules file assigned to GameProgressTracker");
            }
        }
        
        /// <summary>
        /// Handles analytics tracking events and updates game state
        /// </summary>
        private void HandleTrackEvent(string name, Dictionary<string, object> arguments, string category)
        {
            // Store event data in the state
            StoreEventData(name, arguments, category);
            
            // Process rules with the updated state
            ProcessRules();
        }
        
        /// <summary>
        /// Stores event data in the game state for rules to process
        /// </summary>
        private void StoreEventData(string eventName, Dictionary<string, object> arguments, string category)
        {
            // Store basic event information
            _gameState.SetValue("LastEventName", eventName);
            _gameState.SetValue("LastEventCategory", category);
            
            // Store all event arguments directly in the state
            // This allows rules to access any argument without special handling
            if (arguments != null)
            {
                foreach (var arg in arguments)
                {
                    _gameState.SetValue("Event_" + arg.Key, arg.Value);
                }
            }
            
            // Log the event
            Debug.Log($"Event stored: {eventName}, Category: {category}");
        }
        
        /// <summary>
        /// Process all rules against the current game state
        /// </summary>
        private void ProcessRules()
        {
            // Get current state
            var currentState = _gameState.GetCurrentState();
            
            // Execute rules
            var results = _ruleEngine.ExecuteRules(currentState);
            
            // Update game state with results
            _gameState.UpdateState(results);
            
            // Process rewards if any
            if (results.ContainsKey("Reward"))
            {
                _rewardHandler.ProcessReward(results["Reward"]);
            }
            
            // Check for milestone achievements
            if (results.ContainsKey("MilestoneReached"))
            {
                string milestone = results["MilestoneReached"].ToString();
                Debug.Log($"Milestone achieved: {milestone}");
                
                // Trigger UI notification or other game events
                OnMilestoneAchieved(milestone);
            }
        }
        
        /// <summary>
        /// Event triggered when a milestone is achieved
        /// </summary>
        private void OnMilestoneAchieved(string milestone)
        {
            // Broadcast milestone achievement to other systems
            // For example, show UI notification, play effects, etc.
        }
        
        /// <summary>
        /// Save game state to PlayerPrefs
        /// </summary>
        public void SaveGameState()
        {
            string stateJson = JsonConvert.SerializeObject(_gameState.GetCurrentState());
            PlayerPrefs.SetString("GameProgressState", stateJson);
            PlayerPrefs.Save();
        }
        
        /// <summary>
        /// Load game state from PlayerPrefs
        /// </summary>
        public void LoadGameState()
        {
            if (PlayerPrefs.HasKey("GameProgressState"))
            {
                string stateJson = PlayerPrefs.GetString("GameProgressState");
                var savedState = JsonConvert.DeserializeObject<Dictionary<string, object>>(stateJson);
                _gameState.LoadState(savedState);
            }
        }
    }
} 