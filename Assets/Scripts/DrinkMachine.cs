using UnityEngine;

/// <summary>
/// Dispenses drinks into compatible cups held by the player.
/// Validates cup type compatibility and provides audio/visual feedback.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class DrinkMachine : MonoBehaviour
{
    #region Serialized Fields
    
    [Header("Drink Configuration")]
    [Tooltip("Type of drink this machine dispenses")]
    [SerializeField] private Cup.DrinkType drinkType;
    
    [Header("Audio")]
    [Tooltip("Sound played when successfully pouring drink")]
    [SerializeField] private AudioClip pourSound;
    
    [Tooltip("Sound played on error (wrong cup, already full, etc.)")]
    [SerializeField] private AudioClip errorSound;
    
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
            Debug.LogWarning($"[DrinkMachine] No AudioSource found on {gameObject.name}");
        }
    }
    
    #endregion
    
    #region Public API
    
    /// <summary>
    /// Attempts to fill the player's held cup with this machine's drink.
    /// Called by PlayerInteraction when player presses E.
    /// </summary>
    public void Interact(PlayerInteraction player)
    {
        // Validation chain
        if (!ValidatePlayerHoldingItem(player)) return;
        if (!ValidateItemIsCup(player, out Cup cup)) return;
        if (!ValidateCupIsEmpty(cup)) return;
        if (!ValidateCupTypeCompatibility(cup)) return;
        
        // Success - dispense drink
        DispenseDrink(cup);
    }
    
    #endregion
    
    #region Validation
    
    private bool ValidatePlayerHoldingItem(PlayerInteraction player)
    {
        if (player.CurrentHeldItem != null)
        {
            return true;
        }
        
        ShowErrorFeedback("You need to hold a cup first!");
        return false;
    }
    
    private bool ValidateItemIsCup(PlayerInteraction player, out Cup cup)
    {
        cup = player.CurrentHeldItem.GetComponent<Cup>();
        
        if (cup != null)
        {
            return true;
        }
        
        ShowErrorFeedback("That's not a cup!");
        return false;
    }
    
    private bool ValidateCupIsEmpty(Cup cup)
    {
        if (cup.contents == Cup.DrinkType.None)
        {
            return true;
        }
        
        ShowErrorFeedback("This cup is already full!");
        PlayErrorSound();
        return false;
    }
    
    private bool ValidateCupTypeCompatibility(Cup cup)
    {
        if (IsCupCompatibleWithDrink(cup.cupType, drinkType))
        {
            return true;
        }
        
        string correctCupName = GetRequiredCupTypeName(drinkType);
        ShowErrorFeedback($"Wrong cup! Use a {correctCupName} for {drinkType}!");
        PlayErrorSound();
        return false;
    }
    
    #endregion
    
    #region Drink Dispensing
    
    private void DispenseDrink(Cup cup)
    {
        PlayPourSound();
        FillCup(cup);
        ShowSuccessFeedback();
    }
    
    private void FillCup(Cup cup)
    {
        cup.Fill(drinkType);
    }
    
    #endregion
    
    #region Cup Compatibility System
    
    private bool IsCupCompatibleWithDrink(Cup.CupType cupType, Cup.DrinkType drinkType)
    {
        Cup.CupType requiredCupType = GetRequiredCupType(drinkType);
        
        // If no specific requirement, allow any cup
        if (requiredCupType == Cup.CupType.Coffee && 
            drinkType == Cup.DrinkType.None)
        {
            return true;
        }
        
        return cupType == requiredCupType;
    }
    
    private Cup.CupType GetRequiredCupType(Cup.DrinkType drinkType)
    {
        switch (drinkType)
        {
            case Cup.DrinkType.Coffee:
            case Cup.DrinkType.Cappuccino:
            case Cup.DrinkType.Latte:
            case Cup.DrinkType.Mocha:
            case Cup.DrinkType.Espresso:
                return Cup.CupType.Coffee;
            
            case Cup.DrinkType.Beer:
                return Cup.CupType.Beer;
            
            case Cup.DrinkType.Takeaway:
                return Cup.CupType.Takeaway;
            
            default:
                return Cup.CupType.Coffee; // Default fallback
        }
    }
    
    private string GetRequiredCupTypeName(Cup.DrinkType drinkType)
    {
        switch (drinkType)
        {
            case Cup.DrinkType.Coffee:
            case Cup.DrinkType.Cappuccino:
            case Cup.DrinkType.Latte:
            case Cup.DrinkType.Mocha:
            case Cup.DrinkType.Espresso:
                return "Coffee Cup";
            
            case Cup.DrinkType.Beer:
                return "Beer Mug";
            
            case Cup.DrinkType.Takeaway:
                return "Takeaway Cup";
            
            default:
                return "proper cup";
        }
    }
    
    #endregion
    
    #region Audio
    
    private void PlayPourSound()
    {
        if (audioSource != null && pourSound != null)
        {
            audioSource.PlayOneShot(pourSound);
        }
    }
    
    private void PlayErrorSound()
    {
        if (audioSource != null && errorSound != null)
        {
            audioSource.PlayOneShot(errorSound);
        }
    }
    
    #endregion
    
    #region Feedback
    
    private void ShowSuccessFeedback()
    {
        FeedbackManager.Instance?.ShowMessage(
            $"Filled cup with {drinkType}", 
            FeedbackManager.MessageType.Success
        );
    }
    
    private void ShowErrorFeedback(string message)
    {
        FeedbackManager.Instance?.ShowMessage(
            message, 
            FeedbackManager.MessageType.Error
        );
    }
    
    #endregion
    
    #region Public Utility Methods
    
    /// <summary>
    /// Check if a specific cup type can be used with this machine
    /// </summary>
    public bool CanAcceptCupType(Cup.CupType cupType)
    {
        return IsCupCompatibleWithDrink(cupType, drinkType);
    }
    
    /// <summary>
    /// Get the drink type this machine dispenses
    /// </summary>
    public Cup.DrinkType GetDrinkType()
    {
        return drinkType;
    }
    
    #endregion
}