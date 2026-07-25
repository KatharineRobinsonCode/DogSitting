using UnityEngine;
using System.Collections.Generic;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    [Header("Torch")]
   private GameObject playerTorch;

    [Header("Squeaky Toy")]
    private AudioSource squeakyToyAudio;
    private AudioClip squeakyToyClip;
    private Dog brinkley;

    [Header("UI")]
    private InventoryWheel inventoryWheel;

    [Header("All Items (for save/load)")]
    [SerializeField] private List<InventoryItemData> allItems;

    private List<InventoryItemData> items = new List<InventoryItemData>();
    private InventoryItemData activeItem;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadInventory();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
            inventoryWheel.ToggleWheel(items);

        if (Input.GetKeyDown(KeyCode.F))
            UseActiveItem();
    }

    public void AddItem(InventoryItemData item)
    {
        items.Add(item);
        Debug.Log($"[Inventory] Added {item.itemName}");

        if (item.itemType == ItemType.Torch && playerTorch != null)
            playerTorch.SetActive(true);

        inventoryWheel.RefreshIfOpen(items);
        SaveInventory();
    }

    public bool HasItem(ItemType type)
    {
        return items.Exists(i => i.itemType == type);
    }

    public void SetActiveItem(InventoryItemData item)
    {
        activeItem = item;
        Debug.Log($"[Inventory] Active item set to {item.itemName}");
    }

    public void ClearInventory()
    {
        items.Clear();
        activeItem = null;
        PlayerPrefs.DeleteKey("InventoryItems");
        PlayerPrefs.Save();
        Debug.Log("[Inventory] Inventory cleared");
    }

    private void SaveInventory()
    {
        string itemTypes = string.Join(",", items.ConvertAll(i => i.itemType.ToString()));
        PlayerPrefs.SetString("InventoryItems", itemTypes);
        PlayerPrefs.Save();
        Debug.Log($"[Inventory] Saved inventory: {itemTypes}");
    }

    private void LoadInventory()
    {
        string saved = PlayerPrefs.GetString("InventoryItems", "");
        if (string.IsNullOrEmpty(saved)) return;

        foreach (string typeStr in saved.Split(','))
        {
            if (System.Enum.TryParse(typeStr, out ItemType type))
            {
                InventoryItemData match = allItems?.Find(i => i.itemType == type);
                if (match != null)
                {
                    items.Add(match);
                    // Re-enable torch if it was in inventory
                    if (type == ItemType.Torch && playerTorch != null)
                        playerTorch.SetActive(true);
                    Debug.Log($"[Inventory] Restored {match.itemName} from save");
                }
            }
        }
    }

    private void UseActiveItem()
    {
        if (activeItem == null) return;

        switch (activeItem.itemType)
        {
            case ItemType.Torch:
                break;

            case ItemType.SqueakyToy:
                UseSqueakyToy();
                break;

            case ItemType.Knife:
                Debug.Log("[Inventory] Knife used");
                break;

            case ItemType.Collar:
                Debug.Log("[Inventory] Collar used");
                break;
        }
    }

    private void UseSqueakyToy()
    {
        if (squeakyToyAudio != null && squeakyToyClip != null)
            squeakyToyAudio.PlayOneShot(squeakyToyClip);

        if (brinkley != null)
            brinkley.ComeToPlayer();

        if (StoryFlags.Instance != null)
            StoryFlags.Instance.SetUsedSqueakyToy();

        Debug.Log("[Inventory] Squeaky toy used — calling Brinkley");
    }
}