using UnityEngine;

public class Cup : MonoBehaviour
{
    public enum CupType { DraftBeer, TakeawayBeer, Spirit }
    public enum DrinkType { None, DraftBeer, TakeawayBeer, Spirit }

    [Header("Cup Configuration")]
    public CupType cupType;
    public DrinkType contents = DrinkType.None;

    [Header("Liquid Visual Models")]
    public GameObject beerModel;
    public GameObject takeawayModel;
    public GameObject spiritModel;

    // Spawner reference
    private CupSpawner spawner;

    private void Start()
    {
        spawner = GetComponentInParent<CupSpawner>();
        UpdateVisuals();
    }

    public void Fill(DrinkType drink)
    {
        contents = drink;
        UpdateVisuals();
    }

    public void UpdateVisuals()
    {
        if (beerModel != null) beerModel.SetActive(contents == DrinkType.DraftBeer);
        if (takeawayModel != null) takeawayModel.SetActive(contents == DrinkType.TakeawayBeer);
        if (spiritModel != null) spiritModel.SetActive(contents == DrinkType.Spirit);
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

    public void OnPickedUp()
    {
        if (spawner != null)
            spawner.OnCupTaken();
    }
}