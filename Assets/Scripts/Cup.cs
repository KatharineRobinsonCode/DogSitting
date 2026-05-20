using UnityEngine;
using System.Collections.Generic;

public class Cup : MonoBehaviour
{
    #region Enums

    public enum CupType
    {
        DraftBeer,
        TakeawayBeer,
        Spirit
    }

    public enum DrinkType
    {
        None,
        DraftBeer,
        TakeawayBeer,
        Spirit
    }

    public static string GetDisplayName(DrinkType drinkType)
{
    switch (drinkType)
    {
        case DrinkType.DraftBeer: return "Draft Beer";
        case DrinkType.TakeawayBeer: return "Takeaway Beer";
        case DrinkType.Spirit: return "Spirit";
        default: return "None";
    }
}

    #endregion

    #region Serialized Fields

    [Header("Cup Configuration")]
    public CupType cupType;

    [Header("Current Contents")]
    public DrinkType contents = DrinkType.None;

    [Header("Liquid Visual Models")]
    [SerializeField] private GameObject beerModel;
    [SerializeField] private GameObject takeawayModel;
    [SerializeField] private GameObject spiritModel;

    #endregion

    #region Private Fields

    private Dictionary<DrinkType, GameObject> liquidModels;

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        InitializeLiquidModels();
    }

    private void Start()
    {
        UpdateVisuals();
    }

    #endregion

    #region Initialization

    private void InitializeLiquidModels()
    {
        liquidModels = new Dictionary<DrinkType, GameObject>();

        if (beerModel != null)
            liquidModels[DrinkType.DraftBeer] = beerModel;

        if (takeawayModel != null)
            liquidModels[DrinkType.TakeawayBeer] = takeawayModel;

        if (spiritModel != null)
            liquidModels[DrinkType.Spirit] = spiritModel;
    }

    #endregion

    #region Public API

    public void Fill(DrinkType drinkType)
    {
        if (drinkType == DrinkType.None)
        {
            Debug.LogWarning("[Cup] Attempted to fill with DrinkType.None");
            return;
        }

        contents = drinkType;
        UpdateVisuals();
        Debug.Log($"[Cup] Filled with {drinkType}");
    }

    public void Empty()
    {
        contents = DrinkType.None;
        UpdateVisuals();
        Debug.Log("[Cup] Emptied");
    }

    public bool IsEmpty() => contents == DrinkType.None;
    public bool IsFull() => contents != DrinkType.None;
    public DrinkType GetContents() => contents;

    #endregion

    #region Visual Management

    public void UpdateVisuals()
    {
        SetModelActive(beerModel, false);
        SetModelActive(takeawayModel, false);
        SetModelActive(spiritModel, false);

        if (contents != DrinkType.None && liquidModels.TryGetValue(contents, out GameObject model))
            SetModelActive(model, true);
    }

    private void SetModelActive(GameObject model, bool active)
    {
        if (model != null)
            model.SetActive(active);
    }

    #endregion

    #region Validation

    public bool CanHoldDrink(DrinkType drinkType)
    {
        switch (cupType)
        {
            case CupType.DraftBeer:
                return drinkType == DrinkType.DraftBeer;
            case CupType.TakeawayBeer:
                return drinkType == DrinkType.TakeawayBeer;
            case CupType.Spirit:
                return drinkType == DrinkType.Spirit;
            default:
                return false;
        }
    }

    #endregion

    #region Debug

#if UNITY_EDITOR
    private void OnValidate()
    {
        switch (cupType)
        {
            case CupType.DraftBeer:
                if (beerModel == null)
                    Debug.LogWarning($"[Cup] {gameObject.name}: Draft beer cup missing beer model", this);
                break;
            case CupType.TakeawayBeer:
                if (takeawayModel == null)
                    Debug.LogWarning($"[Cup] {gameObject.name}: Takeaway cup missing takeaway model", this);
                break;
            case CupType.Spirit:
                if (spiritModel == null)
                    Debug.LogWarning($"[Cup] {gameObject.name}: Spirit cup missing spirit model", this);
                break;
        }
    }
#endif

    #endregion
}