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

    [Header("3D Pour")]
    [Tooltip("How full the glass LOOKS at the top of the green zone (1 = brim). 0.9 leaves room for foam.")]
    [SerializeField, Range(0.3f, 1f)] private float goodPourLevel = 0.9f;
    [Tooltip("Draw the green target band on the glass")]
    [SerializeField] private bool showTargetLine = true;

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
        Debug.Log($"[DrinkMachine] Interact called — drinkType: {drinkType} isCurrentlyFilling: {isCurrentlyFilling}");
        if (isCurrentlyFilling) return;
        if (!ValidatePlayerHoldingItem(player)) return;
        if (!ValidateItemIsCup(player, out Cup cup)) return;
        if (!ValidateCupIsEmpty(cup)) return;
        if (!ValidateCupTypeCompatibility(cup)) return;

        StartCoroutine(FillingQTE(cup, player));
    }

    #endregion

    #region Filling QTE

    // Maps the logic value (0..1, green zone etc.) to how full the glass LOOKS.
    // Top of the green zone = goodPourLevel, so a correct pour looks like a proper pint.
    private float VisualFill(float logicFill)
    {
        float top = Mathf.Max(greenZoneMax, 0.01f);
        return Mathf.Clamp01(logicFill / top * goodPourLevel);
    }

    private IEnumerator FillingQTE(Cup cup, PlayerInteraction player)
    {
        isCurrentlyFilling = true;
        IsFillingActive = true;

        bool isTakeaway = drinkType == Cup.DrinkType.TakeawayBeer;
        RectTransform openerRect = null;

        if (isTakeaway)
        {
            // Bottle opener still uses the old UI (no liquid to show)
            if (fillingPanel != null)   fillingPanel.SetActive(true);
            if (takeawayBottle != null) takeawayBottle.SetActive(true);
            if (takeawayFillImage != null) openerRect = takeawayFillImage.GetComponent<RectTransform>();

            if (player.CurrentHeldItem != null)
                foreach (Renderer r in player.CurrentHeldItem.GetComponentsInChildren<Renderer>())
                    r.enabled = false;

            if (instructionText != null) instructionText.text = "Hold Q to open bottle";
        }
        else
        {
            // 3D pour: the real held glass fills up, no panels
            cup.PreviewFill(drinkType, 0f);
            if (showTargetLine)
                cup.ShowTargetBand(VisualFill(greenZoneMin), VisualFill(greenZoneMax));
            ShowInfoFeedback("Hold Q to pour");
        }

        float fillSpeed = drinkType == Cup.DrinkType.Spirit     ? SPIRIT_FILL_SPEED
                        : drinkType == Cup.DrinkType.TakeawayBeer ? takeawayFillSpeed
                        : drinkType == Cup.DrinkType.Guinnez    ? GUINNESS_FILL_SPEED
                        : drinkType == Cup.DrinkType.Wine        ? WINE_FILL_SPEED
                        : DRAFT_FILL_SPEED;

        bool overallSucceeded = false;
        float finalFill = 0f;

        while (!overallSucceeded)
        {
            float currentFill = 0f;
            bool qteComplete = false;
            bool succeeded = false;

            if (isTakeaway && openerRect != null)
                openerRect.anchoredPosition = new Vector2(openerRect.anchoredPosition.x, openerStartY);
            else if (!isTakeaway)
                cup.PreviewFill(drinkType, 0f);

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
                    else if (!isTakeaway)
                    {
                        cup.PreviewFill(drinkType, VisualFill(currentFill));
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
                    Debug.Log($"[DrinkMachine] {drinkType} - released at {currentFill:F3} - green zone {greenZoneMin} to {greenZoneMax} - succeeded: {succeeded}");
                    qteComplete = true;
                }

                yield return null;
            }

            yield return new WaitForSeconds(0.4f);

            if (succeeded)
            {
                finalFill = currentFill;
                overallSucceeded = true;
            }
            else
            {
                if (audioSource != null && failSound != null)
                    audioSource.PlayOneShot(failSound);

                ShowErrorFeedback(currentFill >= 1f
                    ? "Oops too much... Let's try that again."
                    : "Okay that's not enough... Let's try that again.");

                if (instructionText != null && isTakeaway)
                    instructionText.text = "Hold Q to open bottle";

                // Drain the glass so the retry starts empty
                if (!isTakeaway)
                {
                    float from = VisualFill(currentFill);
                    for (float t = 0f; t < 0.3f; t += Time.deltaTime)
                    {
                        cup.PreviewFill(drinkType, Mathf.Lerp(from, 0f, t / 0.3f));
                        yield return null;
                    }
                    cup.PreviewFill(drinkType, 0f);
                }

                yield return new WaitForSeconds(0.6f);
            }
        }

        // Clean up
        cup.HideTargetBand();

        if (isTakeaway)
        {
            if (takeawayBottle != null) takeawayBottle.SetActive(false);
            if (fillingPanel != null)   fillingPanel.SetActive(false);
            if (player.CurrentHeldItem != null)
                foreach (Renderer r in player.CurrentHeldItem.GetComponentsInChildren<Renderer>())
                    r.enabled = true;
        }

        if (audioSource != null && successSound != null)
            audioSource.PlayOneShot(successSound);

        IsFillingActive = false;
        cup.Fill(drinkType, isTakeaway ? 1f : VisualFill(finalFill));
        isCurrentlyFilling = false;
    }

    #endregion

    #region Validation

    private bool ValidatePlayerHoldingItem(PlayerInteraction player)
    {
        if (player.CurrentHeldItem != null) return true;
        ShowErrorFeedback("Holding a cup first might be smart...");
        return false;
    }

    private bool ValidateItemIsCup(PlayerInteraction player, out Cup cup)
    {
        cup = player.CurrentHeldItem.GetComponent<Cup>();
        if (cup != null) return true;
        ShowErrorFeedback("That's not gonna work...");
        return false;
    }

    private bool ValidateCupIsEmpty(Cup cup)
    {
        if (cup.contents == Cup.DrinkType.None) return true;
        ShowErrorFeedback("I've already filled the cup...");
        PlayErrorSound();
        return false;
    }

    private bool ValidateCupTypeCompatibility(Cup cup)
    {
        if (IsCupCompatibleWithDrink(cup.cupType, drinkType)) return true;
        string correctCupName = GetRequiredCupTypeName(drinkType);
        ShowErrorFeedback($"Wrong cup... I need to use a {correctCupName} for {drinkType}!");
        PlayErrorSound();
        return false;
    }

    #endregion

    #region Drink Dispensing

    private void DispenseDrink(Cup cup)
    {
        cup.Fill(drinkType);
        //ShowSuccessFeedback();
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
            case Cup.DrinkType.Guinnez:     return cupType == Cup.CupType.Guinnez;
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
            case Cup.DrinkType.Guinnez:     return "Guinnez Glass";
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

    private void ShowInfoFeedback(string message)
    {
        FeedbackManager.Instance?.ShowMessage(
            message,
            FeedbackManager.MessageType.Info);
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