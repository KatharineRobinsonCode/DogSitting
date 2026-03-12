using UnityEngine;

/// <summary>
/// Interactable trash bin that allows players to discard held items.
/// Provides audio and visual feedback when items are discarded.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class Trash : MonoBehaviour
{
    #region Serialized Fields
    
    [Header("Audio")]
    [Tooltip("Sound played when item is discarded")]
    [SerializeField] private AudioClip trashSound;
    
    #endregion
    
    #region Private Fields
    
    private AudioSource audioSource;
    
    #endregion
    
    #region Unity Lifecycle
    
    private void Start()
    {
        InitializeComponents();
    }
    
    #endregion
    
    #region Initialization
    
    private void InitializeComponents()
    {
        audioSource = GetComponent<AudioSource>();
        
        if (audioSource == null)
        {
            Debug.LogWarning($"[Trash] No AudioSource found on {gameObject.name}");
        }
    }
    
    #endregion
    
    #region Public API
    
    /// <summary>
    /// Called by PlayerInteraction when player presses E while looking at trash bin.
    /// Discards the currently held item if one exists.
    /// </summary>
    public void Interact(PlayerInteraction player)
    {
        if (ValidatePlayerHoldingItem(player))
        {
            DiscardItem(player);
        }
        else
        {
            ShowNotHoldingFeedback();
        }
    }
    
    #endregion
    
    #region Validation
    
    private bool ValidatePlayerHoldingItem(PlayerInteraction player)
    {
        return player.CurrentHeldItem != null;
    }
    
    #endregion
    
    #region Item Disposal
    
    private void DiscardItem(PlayerInteraction player)
    {
        PlayTrashSound();
        DestroyHeldItem(player);
        ShowDiscardFeedback();
        
        Debug.Log("[Trash] Item discarded");
    }
    
    private void DestroyHeldItem(PlayerInteraction player)
    {
        if (player.CurrentHeldItem != null)
        {
            Destroy(player.CurrentHeldItem);
        }
    }
    
    #endregion
    
    #region Audio
    
    private void PlayTrashSound()
    {
        if (audioSource != null && trashSound != null)
        {
            audioSource.PlayOneShot(trashSound);
        }
    }
    
    #endregion
    
    #region Feedback
    
    private void ShowDiscardFeedback()
    {
        FeedbackManager.Instance?.ShowMessage(
            "Drink discarded", 
            FeedbackManager.MessageType.Info
        );
    }
    
    private void ShowNotHoldingFeedback()
    {
        FeedbackManager.Instance?.ShowMessage(
            "You're not holding anything!", 
            FeedbackManager.MessageType.Error
        );
    }
    
    #endregion
    
    #region Public Utility Methods
    
    /// <summary>
    /// Checks if player can discard their current item.
    /// </summary>
    public bool CanDiscardItem(PlayerInteraction player)
    {
        return player != null && player.CurrentHeldItem != null;
    }
    
    #endregion
}