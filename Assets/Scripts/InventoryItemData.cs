using UnityEngine;

public enum ItemType { Torch, SqueakyToy, Collar, Knife, Generic }

[CreateAssetMenu(fileName = "NewItem", menuName = "DogSitter/Inventory Item")]
public class InventoryItemData : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    public ItemType itemType;
    public bool isUsableAnywhere;
    [TextArea] public string usePrompt = "Press F to use";
}