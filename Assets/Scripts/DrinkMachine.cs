using UnityEngine;

public class DrinkMachine : MonoBehaviour
{
    private AudioSource audioSource;
    public AudioClip pourSound;
    public AudioClip errorSound; // ADD THIS - drag an error sound in Inspector

    [Header("Machine Settings")]
    public Cup.DrinkType typeToGive; 

    void Start() 
    { 
        audioSource = GetComponent<AudioSource>(); 
    }

    public void Interact(PlayerInteraction player)
    {
        // 1. Check if holding anything
        if (player.currentHeldItem == null)
        {
            FeedbackManager.Instance?.ShowMessage(
                "You need to hold a cup first!", 
                FeedbackManager.MessageType.Error
            );
            return;
        }

        // 2. Check if it's a cup
        Cup cup = player.currentHeldItem.GetComponent<Cup>();
        if (cup == null)
        {
            FeedbackManager.Instance?.ShowMessage(
                "That's not a cup!", 
                FeedbackManager.MessageType.Error
            );
            return;
        }

        // 3. Check if cup is already full
        if (cup.contents != Cup.DrinkType.None)
        {
            FeedbackManager.Instance?.ShowMessage(
                "This cup is already full!", 
                FeedbackManager.MessageType.Error
            );
            
            if (audioSource != null && errorSound != null)
                audioSource.PlayOneShot(errorSound);
            
            return;
        }

        // 4. NEW: Check if cup type matches drink type
        if (!IsCupCompatible(cup.cupType, typeToGive))
        {
            // Wrong cup type!
            string correctCupType = GetCorrectCupType(typeToGive);
            
            FeedbackManager.Instance?.ShowMessage(
                $"Wrong cup! Use a {correctCupType} for {typeToGive}!", 
                FeedbackManager.MessageType.Error
            );
            
            if (audioSource != null && errorSound != null)
                audioSource.PlayOneShot(errorSound);
            
            return;
        }

        // 5. SUCCESS - Fill the cup
        if (audioSource != null && pourSound != null) 
        {
            audioSource.PlayOneShot(pourSound);
        }

        cup.Fill(typeToGive); 
        
        FeedbackManager.Instance?.ShowMessage(
            "Filled cup with " + typeToGive, 
            FeedbackManager.MessageType.Success
        );
    }

    // Check if the cup type is compatible with the drink type
    bool IsCupCompatible(Cup.CupType cupType, Cup.DrinkType drinkType)
    {
        switch (drinkType)
        {
            case Cup.DrinkType.Coffee:
            case Cup.DrinkType.Cappuccino:
            case Cup.DrinkType.Latte:
            case Cup.DrinkType.Mocha:
            case Cup.DrinkType.Espresso:
                // Coffee drinks go in coffee cups
                return cupType == Cup.CupType.Coffee;

            case Cup.DrinkType.Beer:
                // Beer goes in beer mugs
                return cupType == Cup.CupType.Beer;

            case Cup.DrinkType.Takeaway:
                // Takeaway drinks go in takeaway cups
                return cupType == Cup.CupType.Takeaway;

            default:
                return true; // Allow if drink type not specified
        }
    }

    // Get the name of the correct cup type for a drink
    string GetCorrectCupType(Cup.DrinkType drinkType)
    {
        switch (drinkType)
        {
            case Cup.DrinkType.Coffee:
            case Cup.DrinkType.Cappuccino:
            case Cup.DrinkType.Latte:
            case Cup.DrinkType.Mocha:
            case Cup.DrinkType.Espresso:
                return "Coffee Cup";

            case Cup.DrinkType.Beer:
                return "Beer Mug";

            case Cup.DrinkType.Takeaway:
                return "Takeaway Cup";

            default:
                return "proper cup";
        }
    }
}