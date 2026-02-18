using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Represents a drinkable container that can hold different types of beverages.
/// Manages visual representation of liquid contents and validates compatibility.
/// </summary>
public class Cup : MonoBehaviour
{
    #region Enums
    
    /// <summary>
    /// Type of container (determines what drinks it can hold).
    /// </summary>
    public enum CupType
    {
        Coffee,
        Beer,
        Takeaway
    }
    
    /// <summary>
    /// Type of drink that can be contained.
    /// </summary>
    public enum DrinkType
    {
        None,
        Coffee,
        Beer,
        Cappuccino,
        Latte,
        Mocha,
        Espresso,
        Takeaway
    }
    
    #endregion
    
    #region Serialized Fields
    
    [Header("Cup Configuration")]
    [Tooltip("Type of cup (determines compatible drinks)")]
    public CupType cupType;
    
    [Header("Current Contents")]
    [Tooltip("What drink is currently in this cup")]
    public DrinkType contents = DrinkType.None;
    
    [Header("Liquid Visual Models")]
    [Tooltip("Visual model for coffee/espresso drinks")]
    [SerializeField] private GameObject coffeeModel;
    
    [Tooltip("Visual model for beer")]
    [SerializeField] private GameObject beerModel;
    
    [Tooltip("Visual model for cappuccino/latte/mocha")]
    [SerializeField] private GameObject cappuccinoModel;
    
    [Tooltip("Visual model for takeaway drinks")]
    [SerializeField] private GameObject takeawayModel;
    
    #endregion
    
    #region Private Fields
    
    // Cache all liquid models for easy management
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
        
        // Map drink types to their visual models
        if (coffeeModel != null)
        {
            RegisterLiquidModel(DrinkType.Coffee, coffeeModel);
            RegisterLiquidModel(DrinkType.Espresso, coffeeModel);
        }
        
        if (beerModel != null)
        {
            RegisterLiquidModel(DrinkType.Beer, beerModel);
        }
        
        if (cappuccinoModel != null)
        {
            RegisterLiquidModel(DrinkType.Cappuccino, cappuccinoModel);
            RegisterLiquidModel(DrinkType.Latte, cappuccinoModel);
            RegisterLiquidModel(DrinkType.Mocha, cappuccinoModel);
        }
        
        if (takeawayModel != null)
        {
            RegisterLiquidModel(DrinkType.Takeaway, takeawayModel);
        }
    }
    
    private void RegisterLiquidModel(DrinkType drinkType, GameObject model)
    {
        if (!liquidModels.ContainsKey(drinkType))
        {
            liquidModels[drinkType] = model;
        }
    }
    
    #endregion
    
    #region Public API
    
    /// <summary>
    /// Fills the cup with the specified drink type and updates visuals.
    /// </summary>
    /// <param name="drinkType">Type of drink to fill</param>
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
    
    /// <summary>
    /// Empties the cup and hides all liquid visuals.
    /// </summary>
    public void Empty()
    {
        contents = DrinkType.None;
        UpdateVisuals();
        
        Debug.Log("[Cup] Emptied");
    }
    
    /// <summary>
    /// Checks if the cup is currently empty.
    /// </summary>
    public bool IsEmpty()
    {
        return contents == DrinkType.None;
    }
    
    /// <summary>
    /// Checks if the cup is currently full (contains any drink).
    /// </summary>
    public bool IsFull()
    {
        return contents != DrinkType.None;
    }
    
    /// <summary>
    /// Gets the current drink contents.
    /// </summary>
    public DrinkType GetContents()
    {
        return contents;
    }
    
    #endregion
    
    #region Visual Management
    
    /// <summary>
    /// Updates the visual representation of the cup's contents.
    /// Shows the appropriate liquid model and hides others.
    /// </summary>
    public void UpdateVisuals()
    {
        HideAllLiquidModels();
        
        if (contents != DrinkType.None)
        {
            ShowLiquidModel(contents);
        }
    }
    
    private void HideAllLiquidModels()
    {
        // Hide all individual models
        SetModelActive(coffeeModel, false);
        SetModelActive(beerModel, false);
        SetModelActive(cappuccinoModel, false);
        SetModelActive(takeawayModel, false);
    }
    
    private void ShowLiquidModel(DrinkType drinkType)
    {
        if (liquidModels.TryGetValue(drinkType, out GameObject model))
        {
            SetModelActive(model, true);
        }
        else
        {
            Debug.LogWarning($"[Cup] No visual model assigned for {drinkType}");
        }
    }
    
    private void SetModelActive(GameObject model, bool active)
    {
        if (model != null)
        {
            model.SetActive(active);
        }
    }
    
    #endregion
    
    #region Validation
    
    /// <summary>
    /// Checks if this cup type can hold the specified drink type.
    /// </summary>
    public bool CanHoldDrink(DrinkType drinkType)
    {
        switch (cupType)
        {
            case CupType.Coffee:
                return IsCoffeeDrink(drinkType);
            
            case CupType.Beer:
                return drinkType == DrinkType.Beer;
            
            case CupType.Takeaway:
                return drinkType == DrinkType.Takeaway;
            
            default:
                return false;
        }
    }
    
    private bool IsCoffeeDrink(DrinkType drinkType)
    {
        return drinkType == DrinkType.Coffee ||
               drinkType == DrinkType.Cappuccino ||
               drinkType == DrinkType.Latte ||
               drinkType == DrinkType.Mocha ||
               drinkType == DrinkType.Espresso;
    }
    
    #endregion
    
    #region Debug Helpers
    
    #if UNITY_EDITOR
    /// <summary>
    /// Validates that liquid models are properly assigned in the editor.
    /// </summary>
    private void OnValidate()
    {
        // Check for missing models
        if (coffeeModel == null)
        {
            Debug.LogWarning($"[Cup] {gameObject.name}: Coffee model not assigned", this);
        }
        
        if (cupType == CupType.Beer && beerModel == null)
        {
            Debug.LogWarning($"[Cup] {gameObject.name}: Beer cup missing beer model", this);
        }
        
        if (cupType == CupType.Takeaway && takeawayModel == null)
        {
            Debug.LogWarning($"[Cup] {gameObject.name}: Takeaway cup missing takeaway model", this);
        }
    }
    #endif
    
    #endregion
}