using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Examples.UnityIntegration
{
    /// <summary>
    /// Handles processing and distribution of rewards from rule engine results
    /// </summary>
    public class RewardHandler
    {
        // Event that fires when a reward is processed
        public event Action<RewardData> OnRewardProcessed;
        
        /// <summary>
        /// Process a reward object from rule execution results
        /// </summary>
        public void ProcessReward(object rewardData)
        {
            try
            {
                RewardData reward = null;
                
                // Handle different types of reward data
                if (rewardData is JObject rewardJObject)
                {
                    // Convert from JObject
                    reward = rewardJObject.ToObject<RewardData>();
                }
                else if (rewardData is Dictionary<string, object> rewardDict)
                {
                    // Convert from Dictionary
                    string json = JsonConvert.SerializeObject(rewardDict);
                    reward = JsonConvert.DeserializeObject<RewardData>(json);
                }
                else if (rewardData is string rewardJson)
                {
                    // Parse from JSON string
                    reward = JsonConvert.DeserializeObject<RewardData>(rewardJson);
                }
                
                if (reward == null)
                {
                    Debug.LogError("Failed to parse reward data");
                    return;
                }
                
                // Log the reward
                Debug.Log($"Player earned reward: {reward.Name}");
                
                // Process based on reward type
                switch (reward.Type)
                {
                    case RewardType.Currency:
                        AwardCurrency(reward);
                        break;
                        
                    case RewardType.Item:
                        AwardItem(reward);
                        break;
                        
                    case RewardType.Character:
                        UnlockCharacter(reward);
                        break;
                        
                    default:
                        Debug.LogWarning($"Unknown reward type: {reward.Type}");
                        break;
                }
                
                // Trigger the event
                OnRewardProcessed?.Invoke(reward);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error processing reward: {ex.Message}");
            }
        }
        
        private void AwardCurrency(RewardData reward)
        {
            int amount = reward.Amount;
            Debug.Log($"Awarding {amount} currency to player");
            
            // Call your game's currency system
            // Example: CurrencyManager.Instance.AddCurrency(amount);
        }
        
        private void AwardItem(RewardData reward)
        {
            string itemId = reward.ItemId;
            Debug.Log($"Awarding item {itemId} to player");
            
            // Call your game's inventory system
            // Example: InventoryManager.Instance.AddItem(itemId);
        }
        
        private void UnlockCharacter(RewardData reward)
        {
            string characterId = reward.CharacterId;
            Debug.Log($"Unlocking character {characterId} for player");
            
            // Call your game's character system
            // Example: CharacterManager.Instance.UnlockCharacter(characterId);
        }
    }
    
    /// <summary>
    /// Types of rewards that can be granted
    /// </summary>
    public enum RewardType
    {
        Currency,
        Item,
        Character
    }
    
    /// <summary>
    /// Data structure for rewards
    /// </summary>
    [Serializable]
    public class RewardData
    {
        [JsonProperty("Type")]
        public RewardType Type { get; set; }
        
        [JsonProperty("Name")]
        public string Name { get; set; }
        
        [JsonProperty("Amount")]
        public int Amount { get; set; }
        
        [JsonProperty("ItemId")]
        public string ItemId { get; set; }
        
        [JsonProperty("CharacterId")]
        public string CharacterId { get; set; }
    }
} 