using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Examples.UnityIntegration
{
    /// <summary>
    /// Example script demonstrating how to use the rule engine in a Unity game
    /// </summary>
    public class ExampleUsage : MonoBehaviour
    {
        [SerializeField] private Button collectBluePieceButton;
        [SerializeField] private Text bluePieceCountText;
        [SerializeField] private Text rewardText;
        [SerializeField] private TextAsset rulesFile; // Reference to the rules file
        
        private void Start()
        {
            // Make sure the required managers are set up
            SetupManagers();
            
            // Add button listener
            collectBluePieceButton.onClick.AddListener(OnCollectBluePieceClicked);
            
            // Listen for rewards
            var rewardHandler = FindObjectOfType<GameProgressTracker>().GetComponent<RewardHandler>();
            if (rewardHandler != null)
            {
                rewardHandler.OnRewardProcessed += OnRewardProcessed;
            }
            
            // Update UI
            UpdateUI();
        }
        
        private void SetupManagers()
        {
            // Check if managers exist in the scene, if not create them
            if (FindObjectOfType<AnalyticsManager>() == null)
            {
                GameObject analyticsObj = new GameObject("AnalyticsManager");
                analyticsObj.AddComponent<AnalyticsManager>();
            }
            
            if (FindObjectOfType<GameProgressTracker>() == null)
            {
                GameObject progressObj = new GameObject("GameProgressTracker");
                var tracker = progressObj.AddComponent<GameProgressTracker>();
                
                // Assign the rules file
                var trackerField = typeof(GameProgressTracker).GetField("rulesFile", 
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                if (trackerField != null && rulesFile != null)
                {
                    trackerField.SetValue(tracker, rulesFile);
                }
            }
        }
        
        /// <summary>
        /// Called when the collect blue piece button is clicked
        /// </summary>
        private void OnCollectBluePieceClicked()
        {
            // Track the blue piece collection through the analytics system
            AnalyticsManager.Instance.TrackBluePieceCollected();
            
            // Update UI
            UpdateUI();
            
            // Show collection effect
            StartCoroutine(ShowCollectionEffect());
        }
        
        /// <summary>
        /// Update the UI with current game state
        /// </summary>
        private void UpdateUI()
        {
            // Get the current count from the game state
            var gameState = FindObjectOfType<GameProgressTracker>().GetComponent<GameState>();
            if (gameState != null)
            {
                int count = gameState.GetValue<int>("BluePiecesCollected");
                bluePieceCountText.text = $"Blue Pieces: {count}";
            }
        }
        
        /// <summary>
        /// Called when a reward is processed
        /// </summary>
        private void OnRewardProcessed(RewardData reward)
        {
            // Show reward notification
            rewardText.text = $"Reward: {reward.Name}";
            rewardText.gameObject.SetActive(true);
            
            // Hide after a delay
            StartCoroutine(HideRewardText(5f));
        }
        
        /// <summary>
        /// Show a visual effect when collecting a piece
        /// </summary>
        private IEnumerator ShowCollectionEffect()
        {
            // Example visual effect
            collectBluePieceButton.interactable = false;
            collectBluePieceButton.GetComponent<Image>().color = Color.blue;
            
            yield return new WaitForSeconds(0.2f);
            
            collectBluePieceButton.GetComponent<Image>().color = Color.white;
            collectBluePieceButton.interactable = true;
        }
        
        /// <summary>
        /// Hide the reward text after a delay
        /// </summary>
        private IEnumerator HideRewardText(float delay)
        {
            yield return new WaitForSeconds(delay);
            rewardText.gameObject.SetActive(false);
        }
        
        private void OnDestroy()
        {
            // Clean up listeners
            if (collectBluePieceButton != null)
            {
                collectBluePieceButton.onClick.RemoveListener(OnCollectBluePieceClicked);
            }
            
            var rewardHandler = FindObjectOfType<GameProgressTracker>().GetComponent<RewardHandler>();
            if (rewardHandler != null)
            {
                rewardHandler.OnRewardProcessed -= OnRewardProcessed;
            }
        }
    }
} 