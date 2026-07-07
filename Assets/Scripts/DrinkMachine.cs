using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class DrinkMachine : MonoBehaviour
{
    #region Serialized Fields

    [Header("Drink Configuration")]
    [SerializeField] private Cup.DrinkType drinkType;

    [Header("Audio")]
    [SerializeField] private AudioClip pourSound;
    [SerializeField] private AudioClip errorSound;
    [SerializeField] private AudioClip successSound;
    [SerializeField] private AudioClip failSound;

    [Header("Filling QTE UI")]
    [SerializeField] private GameObject fillingPanel;
    [SerializeField] private Image liquidFillImage;
    [SerializeField] private TextMeshProUGUI instructionText;

    [Header("Fill Settings")]
    [SerializeField] private float greenZoneMin = 0.65f;
    [SerializeField] private float greenZoneMax = 0.80f;

    #endregion

    #region Private Fields

    private AudioSource audioSource;
    private bool isCurrentlyFilling = false;

    // Fill speeds per drink type
    private const float DRAFT_FILL_SPEED = 0.12f;
    private const float SPIRIT_FILL_SPEED = 0.45f;

    #endregion

    #region Unity Lifecycle

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (fillingPanel != null) fillingPanel.SetActive(false);
        if (liquidFillImage != null) liquidFillImage.fillAmount = 0f;
    }

    #endregion

    #region Public API

    public void Interact(PlayerInteraction player)
    {
        if (isCurrentlyFilling) return;

        if (!ValidatePlayerHoldingItem(player)) return;
        if (!ValidateItemIsCup(player, out Cup cup)) return;
        if (!ValidateCupIsEmpty(cup)) return;
        if (!ValidateCupTypeCompatibility(cup)) return;

        // Takeaway beer — no QTE needed
        if (drinkType == Cup.DrinkType.TakeawayBeer)
        {
            DispenseDrink(cup);
            return;
        }

        StartCoroutine(FillingQTE(cup));
    }

    #endregion

    #region Filling QTE

    private IEnumerator FillingQTE(Cup cup)
    {
        isCurrentlyFilling = true;

        // Show panel
        if (fillingPanel != null) fillingPanel.SetActive(true);
        if (liquidFillImage != null) liquidFillImage.fillAmount = 0f;
        if (instructionText != null) instructionText.text = "Hold Q to pour — release in the green zone!";

        float fillSpeed = drinkType == Cup.DrinkType.Spirit ? SPIRIT_FILL_SPEED : DRAFT_FILL_SPEED;
        float currentFill = 0f;
        bool qteComplete = false;
        bool succeeded = false;

        while (!qteComplete)
        {
            if (Input.GetKey(KeyCode.Q))
            {
                // Fill while Q is held
                currentFill += fillSpeed * Time.deltaTime;
                currentFill = Mathf.Clamp01(currentFill);

                if (liquidFillImage != null)
                    liquidFillImage.fillAmount = currentFill;

                // Play pour sound if not already playing
                if (audioSource != null && pourSound != null && !audioSource.isPlaying)
                    audioSource.PlayOneShot(pourSound);

                // Auto-fail if overfilled
                if (currentFill >= 1f)
                {
                    succeeded = false;
                    qteComplete = true;
                }
            }
            else if (Input.GetKeyUp(KeyCode.Q))
            {
                // Released Q — check if in green zone
                succeeded = currentFill >= greenZoneMin && currentFill <= greenZoneMax;
                qteComplete = true;
            }

            yield return null;
        }

        // Brief pause so player sees where they stopped
        yield return new WaitForSeconds(0.4f);

        // Hide panel
        if (fillingPanel != null) fillingPanel.SetActive(false);
        if (liquidFillImage != null) liquidFillImage.fillAmount = 0f;

        if (succeeded)
        {
            Debug.Log("[DrinkMachine] QTE succeeded");
            if (audioSource != null && successSound != null)
                audioSource.PlayOneShot(successSound);
            DispenseDrink(cup);
        }
        else
        {
            Debug.Log("[DrinkMachine] QTE failed — too early or overfilled");
            if (audioSource != null && failSound != null)
                audioSource.PlayOneShot(failSound);
            ShowErrorFeedback(currentFill >= 1f
                ? "Too much! Try again."
                : "Not enough! Try again.");
        }

        isCurrentlyFilling = false;
    }

    #endregion

    #region Validation

    private bool ValidatePlayerHoldingItem(PlayerInteraction player)
    {
        if (player.CurrentHeldItem != null) return true;
        ShowErrorFeedback("You need to hold a cup first!");
        return false;
    }

    private bool ValidateItemIsCup(PlayerInteraction player, out Cup cup)
    {
        cup = player.CurrentHeldItem.GetComponent<Cup>();
        if (cup != null) return true;
        ShowErrorFeedback("That's not a cup!");
        return false;
    }

    private bool ValidateCupIsEmpty(Cup cup)
    {
        if (cup.contents == Cup.DrinkType.None) return true;
        ShowErrorFeedback("This cup is already full!");
        PlayErrorSound();
        return false;
    }

    private bool ValidateCupTypeCompatibility(Cup cup)
    {
        if (IsCupCompatibleWithDrink(cup.cupType, drinkType)) return true;
        string correctCupName = GetRequiredCupTypeName(drinkType);
        ShowErrorFeedback($"Wrong cup! Use a {correctCupName} for {drinkType}!");
        PlayErrorSound();
        return false;
    }

    #endregion

    #region Drink Dispensing

    private void DispenseDrink(Cup cup)
    {
        cup.Fill(drinkType);
        ShowSuccessFeedback();
    }

    #endregion

    #region Cup Compatibility

    private bool IsCupCompatibleWithDrink(Cup.CupType cupType, Cup.DrinkType drinkType)
    {
        switch (drinkType)
        {
            case Cup.DrinkType.DraftBeer:    return cupType == Cup.CupType.DraftBeer;
            case Cup.DrinkType.TakeawayBeer: return cupType == Cup.CupType.TakeawayBeer;
            case Cup.DrinkType.Spirit:       return cupType == Cup.CupType.Spirit;
            default: return false;
        }
    }

    private string GetRequiredCupTypeName(Cup.DrinkType drinkType)
    {
        switch (drinkType)
        {
            case Cup.DrinkType.DraftBeer:    return "Beer Glass";
            case Cup.DrinkType.TakeawayBeer: return "Takeaway Cup";
            case Cup.DrinkType.Spirit:       return "Spirit Glass";
            default: return "correct glass";
        }
    }

    #endregion

    #region Audio

    private void PlayErrorSound()
    {
        if (audioSource != null && errorSound != null)
            audioSource.PlayOneShot(errorSound);
    }

    #endregion

    #region Feedback

    private void ShowSuccessFeedback()
    {
        FeedbackManager.Instance?.ShowMessage(
            $"Filled cup with {Cup.GetDisplayName(drinkType)}",
            FeedbackManager.MessageType.Success);
    }

    private void ShowErrorFeedback(string message)
    {
        FeedbackManager.Instance?.ShowMessage(
            message,
            FeedbackManager.MessageType.Error);
    }

    #endregion

    #region Public Utility

    public bool CanAcceptCupType(Cup.CupType cupType) =>
        IsCupCompatibleWithDrink(cupType, drinkType);

    public Cup.DrinkType GetDrinkType() => drinkType;

    #endregion
}