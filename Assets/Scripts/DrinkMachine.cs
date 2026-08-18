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
    [SerializeField] private GameObject draftBeerGlass;
    [SerializeField] private GameObject spiritGlass;
    [SerializeField] private GameObject takeawayBottle;
    [SerializeField] private GameObject guinnessGlass; 
    [SerializeField] private GameObject wineGlass;  
    [SerializeField] private Image draftBeerFillImage;
    [SerializeField] private Image spiritFillImage;
    [SerializeField] private Image takeawayFillImage;
    [SerializeField] private Image guinnessFillImage;  
    [SerializeField] private Image wineFillImage; 
    [SerializeField] private TextMeshProUGUI instructionText;

    [Header("Fill Settings")]
    [SerializeField] private float greenZoneMin = 0.65f;
    [SerializeField] private float greenZoneMax = 0.80f;
    [SerializeField] private float takeawayFillSpeed = 0.2f;

    [Header("Takeaway Bottle Opener Settings")]
    [SerializeField] private float openerStartY = 150f;
    [SerializeField] private float openerEndY = 0f;

    #endregion

    #region Private Fields

    private AudioSource audioSource;
    private bool isCurrentlyFilling = false;

    private const float DRAFT_FILL_SPEED = 0.12f;
    private const float SPIRIT_FILL_SPEED = 0.45f;
    private const float GUINNESS_FILL_SPEED = 0.08f;
    private const float WINE_FILL_SPEED = 0.35f;

    #endregion

    #region Unity Lifecycle

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (fillingPanel != null) fillingPanel.SetActive(false);
        if (draftBeerFillImage != null)   draftBeerFillImage.fillAmount = 0f;
        if (spiritFillImage != null)      spiritFillImage.fillAmount = 0f;
        if (guinnessFillImage != null)     guinnessFillImage.fillAmount = 0f;
        if (wineFillImage != null)        wineFillImage.fillAmount = 0f;
    }

    #endregion

    #region Public API

    public static bool IsFillingActive { get; private set; }

    public void Interact(PlayerInteraction player)
    {
        if (isCurrentlyFilling) return;

        if (!ValidatePlayerHoldingItem(player)) return;
        if (!ValidateItemIsCup(player, out Cup cup)) return;
        if (!ValidateCupIsEmpty(cup)) return;
        if (!ValidateCupTypeCompatibility(cup)) return;

        StartCoroutine(FillingQTE(cup, player));
    }

    #endregion

    #region Filling QTE

    private IEnumerator FillingQTE(Cup cup, PlayerInteraction player)
    {
        isCurrentlyFilling = true;
        IsFillingActive = true;

        // Activate the correct glass visual
        if (draftBeerGlass != null)  draftBeerGlass.SetActive(drinkType == Cup.DrinkType.DraftBeer);
        if (spiritGlass != null)     spiritGlass.SetActive(drinkType == Cup.DrinkType.Spirit);
        if (takeawayBottle != null)  takeawayBottle.SetActive(drinkType == Cup.DrinkType.TakeawayBeer);
        if (guinnessGlass != null)   guinnessGlass.SetActive(drinkType == Cup.DrinkType.Guinness);
        if (wineGlass != null)       wineGlass.SetActive(drinkType == Cup.DrinkType.Wine);

        if (fillingPanel != null) fillingPanel.SetActive(true);

        // Hide held cup during QTE
        if (player.CurrentHeldItem != null)
        {
            foreach (Renderer r in player.CurrentHeldItem.GetComponentsInChildren<Renderer>())
                r.enabled = false;
        }

        // Determine which fill image to use
        Image liquidFillImage = drinkType == Cup.DrinkType.Spirit    ? spiritFillImage
                              : drinkType == Cup.DrinkType.Guinness   ? guinnessFillImage
                              : drinkType == Cup.DrinkType.Wine       ? wineFillImage
                              : draftBeerFillImage;

        bool isTakeaway = drinkType == Cup.DrinkType.TakeawayBeer;
        RectTransform openerRect = isTakeaway && takeawayFillImage != null
            ? takeawayFillImage.GetComponent<RectTransform>()
            : null;

        if (!isTakeaway && liquidFillImage != null)
            liquidFillImage.fillAmount = 0f;

        if (isTakeaway && openerRect != null)
        {
            openerRect.anchoredPosition = new Vector2(openerRect.anchoredPosition.x, openerStartY);
            instructionText.text = "Hold Q to open bottle";
        }
        else
        {
            instructionText.text = "Hold Q to pour";
        }

        float fillSpeed = drinkType == Cup.DrinkType.Spirit     ? SPIRIT_FILL_SPEED
                        : drinkType == Cup.DrinkType.TakeawayBeer ? takeawayFillSpeed
                        : drinkType == Cup.DrinkType.Guinness    ? GUINNESS_FILL_SPEED
                        : drinkType == Cup.DrinkType.Wine        ? WINE_FILL_SPEED
                        : DRAFT_FILL_SPEED;

        bool overallSucceeded = false;

        while (!overallSucceeded)
        {
            float currentFill = 0f;
            bool qteComplete = false;
            bool succeeded = false;

            if (!isTakeaway && liquidFillImage != null)
                liquidFillImage.fillAmount = 0f;
            if (isTakeaway && openerRect != null)
                openerRect.anchoredPosition = new Vector2(openerRect.anchoredPosition.x, openerStartY);

            while (!qteComplete)
            {
                if (Input.GetKey(KeyCode.Q))
                {
                    currentFill += fillSpeed * Time.deltaTime;
                    currentFill = Mathf.Clamp01(currentFill);

                    if (isTakeaway && openerRect != null)
                    {
                        float newY = Mathf.Lerp(openerStartY, openerEndY, currentFill);
                        openerRect.anchoredPosition = new Vector2(openerRect.anchoredPosition.x, newY);
                    }
                    else if (liquidFillImage != null)
                    {
                        liquidFillImage.fillAmount = currentFill;
                    }

                    if (audioSource != null && pourSound != null && !audioSource.isPlaying)
                        audioSource.PlayOneShot(pourSound);

                    if (currentFill >= 1f)
                    {
                        succeeded = false;
                        qteComplete = true;
                    }
                }
            else if (Input.GetKeyUp(KeyCode.Q))
{
    succeeded = currentFill >= greenZoneMin && currentFill <= greenZoneMax;
    Debug.Log($"[DrinkMachine] {drinkType} — Released at {currentFill} — greenZone: {greenZoneMin} to {greenZoneMax} — succeeded: {succeeded}");
    qteComplete = true;
}

                yield return null;
            }

            yield return new WaitForSeconds(0.4f);

            if (succeeded)
            {
                overallSucceeded = true;
            }
            else
            {
                if (audioSource != null && failSound != null)
                    audioSource.PlayOneShot(failSound);

                ShowErrorFeedback(currentFill >= 1f
                    ? "Too far! Try again."
                    : "Not enough! Try again.");

                if (instructionText != null)
                    instructionText.text = isTakeaway
                        ? "Hold Q to open bottle"
                        : "Hold Q until you're in the green zone!";

                yield return new WaitForSeconds(0.6f);
            }
        }

        // Hide everything
        if (draftBeerGlass != null)  draftBeerGlass.SetActive(false);
        if (spiritGlass != null)     spiritGlass.SetActive(false);
        if (takeawayBottle != null)  takeawayBottle.SetActive(false);
        if (guinnessGlass != null)   guinnessGlass.SetActive(false);
        if (wineGlass != null)       wineGlass.SetActive(false);
        if (fillingPanel != null)    fillingPanel.SetActive(false);

        if (audioSource != null && successSound != null)
            audioSource.PlayOneShot(successSound);

        if (player.CurrentHeldItem != null)
        {
            foreach (Renderer r in player.CurrentHeldItem.GetComponentsInChildren<Renderer>())
                r.enabled = true;
        }

        IsFillingActive = false;
        DispenseDrink(cup);
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
            case Cup.DrinkType.Guinness:     return cupType == Cup.CupType.Guinness;
            case Cup.DrinkType.Wine:         return cupType == Cup.CupType.Wine;
            default: return false;
        }
    }

    private string GetRequiredCupTypeName(Cup.DrinkType drinkType)
    {
        switch (drinkType)
        {
            case Cup.DrinkType.DraftBeer:    return "Beer Glass";
            case Cup.DrinkType.TakeawayBeer: return "Takeaway Bottle";
            case Cup.DrinkType.Spirit:       return "Spirit Glass";
            case Cup.DrinkType.Guinness:     return "Guinness Glass";
            case Cup.DrinkType.Wine:         return "Wine Glass";
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