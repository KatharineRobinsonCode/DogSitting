using UnityEngine;

public class Cup : MonoBehaviour
{
    public enum CupType { DraftBeer, TakeawayBeer, Spirit, Guinnez, Wine }
    public enum DrinkType { None, DraftBeer, TakeawayBeer, Spirit, Guinnez, Wine }

    [Header("Cup Configuration")]
    public CupType cupType;
    public DrinkType contents = DrinkType.None;

    [Header("Liquid Visual Models")]
    public GameObject beerModel;
    public GameObject takeawayModel;
    public GameObject spiritModel;
    public GameObject guinnessModel;
    public GameObject wineModel;

    private CupSpawner spawner;
    private LiquidFill liquidFill;
    private float displayFill = 1f;   // how full the liquid looks once served

    private void Start()
    {
        spawner = GetComponentInParent<CupSpawner>();
        liquidFill = GetComponent<LiquidFill>();
        UpdateVisuals();
    }

    public void Fill(DrinkType drink, float level = 1f)
    {
        contents = drink;
        displayFill = Mathf.Clamp01(level);
        UpdateVisuals();
    }

    // --- Used by DrinkMachine during the pour ---

    public void PreviewFill(DrinkType drink, float fill01)
    {
        if (liquidFill == null) liquidFill = GetComponent<LiquidFill>();

        GameObject model = drink == DrinkType.DraftBeer    ? beerModel
                         : drink == DrinkType.TakeawayBeer ? takeawayModel
                         : drink == DrinkType.Spirit       ? spiritModel
                         : drink == DrinkType.Guinnez      ? guinnessModel
                         : drink == DrinkType.Wine         ? wineModel : null;

        if (model != null) model.SetActive(fill01 > 0f);
        if (liquidFill != null) liquidFill.SetFill(fill01);
    }

    public void ShowTargetBand(float minFill01, float maxFill01)
    {
        if (liquidFill == null) liquidFill = GetComponent<LiquidFill>();
        if (liquidFill != null) liquidFill.ShowTargetBand(minFill01, maxFill01);
    }

    public void HideTargetBand()
    {
        if (liquidFill != null) liquidFill.HideTargetBand();
    }

    public void UpdateVisuals()
    {
        if (beerModel != null)     beerModel.SetActive(contents == DrinkType.DraftBeer);
        if (takeawayModel != null) takeawayModel.SetActive(contents == DrinkType.TakeawayBeer);
        if (spiritModel != null)   spiritModel.SetActive(contents == DrinkType.Spirit);
        if (guinnessModel != null) guinnessModel.SetActive(contents == DrinkType.Guinnez);
        if (wineModel != null)     wineModel.SetActive(contents == DrinkType.Wine);

        if (liquidFill == null) liquidFill = GetComponent<LiquidFill>();
        if (liquidFill != null) liquidFill.SetFill(contents == DrinkType.None ? 0f : displayFill);
    }

    public static string GetDisplayName(DrinkType drinkType)
    {
        switch (drinkType)
        {
            case DrinkType.DraftBeer:   return "Draft Beer";
            case DrinkType.TakeawayBeer: return "Takeaway Beer";
            case DrinkType.Spirit:      return "Spirit";
            case DrinkType.Guinnez:    return "Guinnez";
            case DrinkType.Wine:        return "Wine";
            default: return "None";
        }
    }

    public void OnPickedUp()
    {
        if (spawner != null)
            spawner.OnCupTaken();
    }
}