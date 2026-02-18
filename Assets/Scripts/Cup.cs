using UnityEngine;

public class Cup : MonoBehaviour
{
    // NEW: What TYPE of cup is this? (Coffee cup, Beer mug, etc.)
    public enum CupType { Coffee, Beer, Takeaway }
    
    // A 'DrinkType' is a custom list (Enum). It's like a multiple-choice question
    // where the options are None, Coffee, Beer, or Cappuccino.
    public enum DrinkType { None, Coffee, Beer, Cappuccino, Latte, Mocha, Espresso, Takeaway }
    
    // NEW: Set this in the Inspector for each cup prefab
    [Header("Cup Type")]
    public CupType cupType;
    
    // This stores what is currently inside this specific cup.
    public DrinkType contents = DrinkType.None;

    [Header("Liquid Models")]
    // These are references to the "Liquid" meshes inside the cup.
    // Usually, these are just flat circles or cylinders colored to look like liquid.
    public GameObject coffeeModel;
    public GameObject beerModel;
    public GameObject cappuccinoModel;

    // Start runs once when the cup is created in the world
    void Start()
    {
        // Make sure the cup looks empty when the game begins
        UpdateVisuals(); 
    }

    // This function is called by your Coffee Machine or Keg script
    public void Fill(DrinkType type)
    {
        // Change the internal "memory" of what's in the cup
        contents = type;
        
        // Refresh the 3D model so the player can actually see the liquid
        UpdateVisuals(); 
    }

    // This is the "Light Switch" function. It turns models on or off.
    public void UpdateVisuals()
    {
        // 1. CLEAN SLATE: Turn off all liquid models so they don't overlap.
        // It's like clearing a whiteboard before writing a new message.
        if (coffeeModel) coffeeModel.SetActive(false);
        if (beerModel) beerModel.SetActive(false);
        if (cappuccinoModel) cappuccinoModel.SetActive(false);

        // 2. SHOW THE DRINK: Check what the 'contents' variable says, 
        // and turn on the matching 3D model.
        if (contents == DrinkType.Coffee && coffeeModel) coffeeModel.SetActive(true);
        if (contents == DrinkType.Beer && beerModel) beerModel.SetActive(true);
        if (contents == DrinkType.Cappuccino && cappuccinoModel) cappuccinoModel.SetActive(true);
        
        // Note: For Latte, Mocha, Espresso, Takeaway - you can add more models
        // or reuse existing ones if they look similar
    }
}