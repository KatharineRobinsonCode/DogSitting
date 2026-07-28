using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages the player's inventory throughout the entire game.
/// Persists across scenes via DontDestroyOnLoad so items are never lost
/// when moving from Pub → Driving → House.
/// Lives in the MainMenu scene and carries across everything.
/// </summary>
public class InventoryManager : MonoBehaviour
{
    // ── Singleton ────────────────────────────────────────────────────────────
    
    // Static reference so any script can call InventoryManager.Instance
    // without needing a direct reference in the Inspector
    public static InventoryManager Instance { get; private set; }

    // ── Scene-Specific References ─────────────────────────────────────────────
    // These are NOT serialized in the Inspector because InventoryManager lives
    // in MainMenu and these objects don't exist until gameplay scenes load.
    // Instead, each object registers itself via the Set methods below when its
    // scene starts.

    private GameObject playerTorch;       // The torch GameObject on the player — enabled when torch is picked up
    private AudioSource squeakyToyAudio;  // AudioSource that plays the squeak sound
    private Dog brinkley;                 // Reference to Brinkley's Dog script for ComeToPlayer()
    private InventoryWheel inventoryWheel; // The UI wheel — registers itself from whichever scene is active

    // The squeaky toy audio clip IS serialized because it's an asset file,
    // not a scene object — it can be assigned in the Inspector on the MainMenu
    [SerializeField] private AudioClip squeakyToyClip;

    // ── All Items List ────────────────────────────────────────────────────────
    
    // Every InventoryItemData ScriptableObject in the game goes here.
    // Used by LoadInventory() to match saved item type strings back to real item data.
    [Header("All Items (for save/load)")]
    [SerializeField] private List<InventoryItemData> allItems;

    // ── Runtime State ─────────────────────────────────────────────────────────
    
    // The items the player is currently carrying — built up as they pick things up
    private List<InventoryItemData> items = new List<InventoryItemData>();
    
    // Whichever item the player last selected from the inventory wheel
    // This is what gets used when they press F
    private InventoryItemData activeItem;

    // ── Unity Lifecycle ───────────────────────────────────────────────────────

    private void Awake()
    {
        // Singleton pattern — if one already exists, destroy this duplicate
        // This handles the case where the scene is reloaded or the object is created twice
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // This is the first and only instance — claim it
        Instance = this;

        // Keep this GameObject alive when scenes change (Pub → Driving → House)
        // Without this, the inventory would be lost every time a new scene loaded
        DontDestroyOnLoad(gameObject);

        // Restore any previously saved inventory from PlayerPrefs
        // This runs on game start so Continue correctly rebuilds the player's items
        LoadInventory();
    }

    private void Update()
    {
        // Tab opens and closes the inventory wheel phone UI
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            Debug.Log($"[Inventory] Tab pressed — inventoryWheel null: {inventoryWheel == null}");
            // ?. means "only call this if inventoryWheel isn't null" — avoids a crash if the wheel hasn't registered yet
            inventoryWheel?.ToggleWheel(items);
        }

        // F uses whichever item is currently selected as the active item
        if (Input.GetKeyDown(KeyCode.F))
            UseActiveItem();
    }

    // ── Scene Registration Methods ────────────────────────────────────────────
    // Called by scene-specific objects when their scene loads.
    // This is how InventoryManager gets references to objects that don't exist
    // in the MainMenu scene where it was created.

    // Called by Dog.cs Start() in the House scene
    public void SetBrinkley(Dog dog) { brinkley = dog; }

    // Called by ElectricTorchOnOff.cs Start() in the House scene
    public void SetPlayerTorch(GameObject torch) { playerTorch = torch; }

    // Called by InventoryWheel.cs Start() in whichever scene is active
    // Logs confirmation so we can verify the wheel registered successfully
    public void SetInventoryWheel(InventoryWheel wheel)
    {
        inventoryWheel = wheel;
        Debug.Log($"[Inventory] Wheel registered: {wheel != null}");
    }

    // Called by SqueakyToyRegistrar.cs Start() in the House scene
    public void SetSqueakyToyAudio(AudioSource source) { squeakyToyAudio = source; }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>
    /// Adds an item to the inventory when the player picks it up.
    /// Also handles any immediate side effects (e.g. enabling the torch).
    /// </summary>
    public void AddItem(InventoryItemData item)
    {
        // Add the item data to the runtime list
        items.Add(item);
        Debug.Log($"[Inventory] Added {item.itemName}");

        // Special case: the torch is a physical GameObject that needs to be
        // activated immediately so the player can see it and use it
        if (item.itemType == ItemType.Torch && playerTorch != null)
            playerTorch.SetActive(true);

        // If the wheel is currently open, rebuild it to show the new item
        inventoryWheel.RefreshIfOpen(items);

        // Persist the updated inventory to PlayerPrefs so it survives scene changes and quitting
        SaveInventory();
    }

    /// <summary>
    /// Returns true if the player is currently carrying an item of the given type.
    /// Used by other scripts to gate interactions — e.g. FoodBowl checks HasItem(DogFood)
    /// before allowing the bowl to be filled.
    /// </summary>
    public bool HasItem(ItemType type)
    {
        // Exists() searches the list and returns true if any item matches the condition
        return items.Exists(i => i.itemType == type);
    }

    /// <summary>
    /// Sets which item is currently "equipped" — called when the player clicks
    /// an item in the inventory wheel. This item will be used when F is pressed.
    /// </summary>
    public void SetActiveItem(InventoryItemData item)
    {
        activeItem = item;
        Debug.Log($"[Inventory] Active item set to {item.itemName}");
    }

    /// <summary>
    /// Wipes the inventory completely. Called when starting a new game to
    /// ensure the player doesn't carry items from a previous run.
    /// </summary>
    public void ClearInventory()
    {
        items.Clear();      // Empty the runtime list
        activeItem = null;  // Clear the selected item
        
        // Remove the saved inventory from PlayerPrefs so LoadInventory()
        // won't restore it next time
        PlayerPrefs.DeleteKey("InventoryItems");
        PlayerPrefs.Save();
        
        Debug.Log("[Inventory] Inventory cleared");
    }

    // ── Save / Load ───────────────────────────────────────────────────────────

    /// <summary>
    /// Saves the current inventory to PlayerPrefs as a comma-separated string
    /// of item type names. e.g. "Knife,DogFood,Torch"
    /// Called automatically every time an item is picked up.
    /// </summary>
    private void SaveInventory()
    {
        // Convert each item's type enum to a string, then join them with commas
        // e.g. [Knife, DogFood] becomes "Knife,DogFood"
        string itemTypes = string.Join(",", items.ConvertAll(i => i.itemType.ToString()));
        
        PlayerPrefs.SetString("InventoryItems", itemTypes);
        PlayerPrefs.Save(); // Flush to disk immediately
        
        Debug.Log($"[Inventory] Saved inventory: {itemTypes}");
    }

    /// <summary>
    /// Restores the inventory from PlayerPrefs when the game starts or continues.
    /// Reads the saved string, splits it back into individual type names,
    /// then finds the matching ScriptableObject from allItems for each one.
    /// </summary>
    private void LoadInventory()
    {
        // Read the saved string — returns empty if nothing was ever saved
        string saved = PlayerPrefs.GetString("InventoryItems", "");
        
        // Nothing saved — new game or first run, nothing to restore
        if (string.IsNullOrEmpty(saved)) return;

        // Split "Knife,DogFood" back into ["Knife", "DogFood"]
        foreach (string typeStr in saved.Split(','))
        {
            // Try to convert the string back to an ItemType enum value
            if (System.Enum.TryParse(typeStr, out ItemType type))
            {
                // Find the ScriptableObject in allItems that matches this type
                // ?. means "only call Find if allItems isn't null"
                InventoryItemData match = allItems?.Find(i => i.itemType == type);
                
                if (match != null)
                {
                    items.Add(match); // Restore the item to the runtime list
                    
                    // Re-enable the torch visually if it was in the saved inventory
                    // (playerTorch might be null here if the House scene hasn't loaded yet —
                    // that's fine, ElectricTorchOnOff.Start() will handle it when the scene loads)
                    if (type == ItemType.Torch && playerTorch != null)
                        playerTorch.SetActive(true);
                    
                    Debug.Log($"[Inventory] Restored {match.itemName} from save");
                }
            }
        }
    }

    // ── Item Usage ────────────────────────────────────────────────────────────

    /// <summary>
    /// Called when the player presses F. Routes to the correct behaviour
    /// based on whichever item is currently active (selected in the wheel).
    /// </summary>
    private void UseActiveItem()
    {
        // Nothing selected — do nothing
        if (activeItem == null) return;

        switch (activeItem.itemType)
        {
            case ItemType.Torch:
                // The torch's own ElectricTorchOnOff script already listens for F
                // and toggles the light itself — nothing extra needed here
                break;

            case ItemType.SqueakyToy:
                UseSqueakyToy(); // Play sound and call Brinkley
                break;

            case ItemType.Knife:
                // Stub — will be wired up to the killer encounter later
                Debug.Log("[Inventory] Knife used");
                break;

            case ItemType.Collar:
                // Stub — will be wired up to a specific scene interaction later
                Debug.Log("[Inventory] Collar used");
                break;
        }
    }

    /// <summary>
    /// Plays the squeaky toy sound and tells Brinkley to walk toward the player.
    /// Also records this in StoryFlags so the ending can remember it happened.
    /// </summary>
    private void UseSqueakyToy()
    {
        // Play the squeak sound — PlayOneShot lets it overlap if pressed repeatedly
        if (squeakyToyAudio != null && squeakyToyClip != null)
            squeakyToyAudio.PlayOneShot(squeakyToyClip);

        // Tell Brinkley's NavMesh agent to path toward the player
        if (brinkley != null)
            brinkley.ComeToPlayer();

        // Record that the squeaky toy was used — checked at the ending
        // to determine whether certain ending branches are available
        if (StoryFlags.Instance != null)
            StoryFlags.Instance.SetUsedSqueakyToy();

        Debug.Log("[Inventory] Squeaky toy used — calling Brinkley");
    }
}