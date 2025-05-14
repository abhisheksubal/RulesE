using System;
using System.Collections.Generic;
using UnityEngine;

namespace Examples.UnityIntegration
{
    /// <summary>
    /// Manages analytics tracking and provides hooks for other systems
    /// </summary>
    public class AnalyticsManager : MonoBehaviour
    {
        // Event that fires when an analytics event is tracked
        public static event Action<string, Dictionary<string, object>, string> OnTrackEvent;
        
        // Singleton instance
        private static AnalyticsManager _instance;
        public static AnalyticsManager Instance => _instance;
        
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
            
            Debug.Log("Analytics Manager initialized");
        }
        
        /// <summary>
        /// Track an event with name, arguments, and category
        /// This is the existing method that we're hooking into
        /// </summary>
        public void MakeTrackCallWithName(string name, Dictionary<string, object> arguments, string category)
        {
            // Log the tracking event
            Debug.Log($"Tracking event: {name}, Category: {category}");
            
            // Send to analytics service
            SendToAnalyticsService(name, arguments, category);
            
            // Trigger the event for other systems to listen to
            OnTrackEvent?.Invoke(name, arguments, category);
        }
        
        #region Item Collection Events
        
        /// <summary>
        /// Helper method for tracking item collection events
        /// </summary>
        public void TrackItemCollected(string itemType, int count = 1)
        {
            var args = new Dictionary<string, object>
            {
                { "item_type", itemType },
                { "count", count }
            };
            
            MakeTrackCallWithName("collect_item", args, "gameplay");
        }
        
        /// <summary>
        /// Helper method for tracking blue piece collection
        /// </summary>
        public void TrackBluePieceCollected(int count = 1)
        {
            TrackItemCollected("blue_piece", count);
        }
        
        /// <summary>
        /// Helper method for tracking red piece collection
        /// </summary>
        public void TrackRedPieceCollected(int count = 1)
        {
            TrackItemCollected("red_piece", count);
        }
        
        /// <summary>
        /// Helper method for tracking green piece collection
        /// </summary>
        public void TrackGreenPieceCollected(int count = 1)
        {
            TrackItemCollected("green_piece", count);
        }
        
        #endregion
        
        #region Level Events
        
        /// <summary>
        /// Track level start event
        /// </summary>
        public void TrackLevelStart(int levelId, string levelName)
        {
            var args = new Dictionary<string, object>
            {
                { "level_id", levelId },
                { "level_name", levelName }
            };
            
            MakeTrackCallWithName("level_start", args, "gameplay");
        }
        
        /// <summary>
        /// Track level complete event
        /// </summary>
        public void TrackLevelComplete(int levelId, int score, float timeSpent)
        {
            var args = new Dictionary<string, object>
            {
                { "level_id", levelId },
                { "score", score },
                { "time_spent", timeSpent }
            };
            
            MakeTrackCallWithName("level_complete", args, "gameplay");
        }
        
        #endregion
        
        #region Player Events
        
        /// <summary>
        /// Track player death event
        /// </summary>
        public void TrackPlayerDeath(string causeOfDeath, Vector3 position)
        {
            var args = new Dictionary<string, object>
            {
                { "cause", causeOfDeath },
                { "position_x", position.x },
                { "position_y", position.y },
                { "position_z", position.z }
            };
            
            MakeTrackCallWithName("player_death", args, "gameplay");
        }
        
        /// <summary>
        /// Track player level up event
        /// </summary>
        public void TrackPlayerLevelUp(int newLevel, Dictionary<string, int> attributeChanges)
        {
            var args = new Dictionary<string, object>
            {
                { "new_level", newLevel },
                { "attribute_changes", attributeChanges }
            };
            
            MakeTrackCallWithName("player_level_up", args, "progression");
        }
        
        #endregion
        
        /// <summary>
        /// Send data to your actual analytics service
        /// </summary>
        private void SendToAnalyticsService(string name, Dictionary<string, object> arguments, string category)
        {
            // Implement your actual analytics service integration here
            // For example:
            // AnalyticsService.TrackEvent(name, arguments, category);
        }
    }
} 