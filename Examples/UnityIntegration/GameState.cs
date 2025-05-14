using System.Collections.Generic;
using UnityEngine;

namespace Examples.UnityIntegration
{
    /// <summary>
    /// Manages the game state data used by the rule engine
    /// </summary>
    public class GameState
    {
        private Dictionary<string, object> _state;

        public GameState()
        {
            _state = new Dictionary<string, object>
            {
                // Counters for various items
                { "BluePiecesCollected", 0 },
                { "RedPiecesCollected", 0 },
                { "GreenPiecesCollected", 0 },
                
                // Event tracking
                { "LastEventName", "" },
                { "LastEventCategory", "" },
                { "LastCollectedItemType", "" },
                { "LastCollectedItemCount", 0 },
                
                // Milestone tracking flags
                { "MilestoneBluePieces10Achieved", false },
                { "MilestoneBluePieces50Achieved", false },
                { "MilestoneBluePieces100Achieved", false }
            };
        }

        /// <summary>
        /// Increment a counter in the state
        /// </summary>
        public void IncrementCounter(string counterName, int amount = 1)
        {
            int currentValue = GetValue<int>(counterName);
            _state[counterName] = currentValue + amount;
        }

        /// <summary>
        /// Get the current value of a state variable
        /// </summary>
        public T GetValue<T>(string key)
        {
            if (_state.TryGetValue(key, out object value))
            {
                if (value is T typedValue)
                {
                    return typedValue;
                }
                
                // Try to convert the value
                try
                {
                    return (T)System.Convert.ChangeType(value, typeof(T));
                }
                catch
                {
                    Debug.LogError($"Failed to convert state value '{key}' to type {typeof(T).Name}");
                    return default;
                }
            }
            
            return default;
        }
        
        /// <summary>
        /// Get the value of a state variable as object
        /// </summary>
        public object GetValue(string key)
        {
            return _state.TryGetValue(key, out object value) ? value : null;
        }

        /// <summary>
        /// Set a state variable
        /// </summary>
        public void SetValue(string key, object value)
        {
            _state[key] = value;
        }
        
        /// <summary>
        /// Set multiple state variables
        /// </summary>
        public void SetValue(string key, Dictionary<string, object> values)
        {
            _state[key] = values;
        }

        /// <summary>
        /// Get a copy of the current state
        /// </summary>
        public Dictionary<string, object> GetCurrentState()
        {
            return new Dictionary<string, object>(_state);
        }

        /// <summary>
        /// Update the state with results from rule execution
        /// </summary>
        public void UpdateState(Dictionary<string, object> ruleResults)
        {
            foreach (var kvp in ruleResults)
            {
                _state[kvp.Key] = kvp.Value;
            }
        }
        
        /// <summary>
        /// Load state from a saved dictionary
        /// </summary>
        public void LoadState(Dictionary<string, object> savedState)
        {
            if (savedState == null) return;
            
            foreach (var kvp in savedState)
            {
                _state[kvp.Key] = kvp.Value;
            }
        }
    }
} 