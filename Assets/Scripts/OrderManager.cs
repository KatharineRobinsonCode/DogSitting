using UnityEngine;
using TMPro; // Needed for the text on the screen
using System.Collections.Generic; // Needed to use 'Queues' (though not fully used here yet!)

public class OrderManager : MonoBehaviour
{
    // THE SINGLETON: This is a coding trick. By making it 'static', any script in 
    // the whole game can talk to 'OrderManager.Instance' without needing a direct link.
    public static OrderManager Instance;
    public string currentRequiredItem;
    
    [Header("UI References")]
    public GameObject orderPanel;   // The background box for the order
    public TextMeshProUGUI orderDisplay; // The actual text saying "1x Coffee"
    
    // A 'Queue' is like a real-life line. You add things to the back and take from the front.
    private Queue<string> orderQueue = new Queue<string>();
    
    // Awake runs even before Start. It's used for setting up the "Singleton"
    void Awake()
    {
        // If there isn't an Instance yet, I'll be the one!
        if (Instance == null) Instance = this;
        // If one already exists, destroy me so there aren't two bosses.
        else Destroy(gameObject);
    }
    
    // Start runs when the game begins
    void Start()
    {
        // Hide the order panel at the start so the screen is clean
        if (orderPanel != null) orderPanel.SetActive(false);
    }
    
    // This is the function NPCs call when they finish talking
    public void ShowOrder(string orderText)
    {
        if (orderPanel != null)
        {
            // Turn the UI box on
            orderPanel.SetActive(true);
            
            // Update the text to match what the customer wants
            if (orderDisplay != null)
            {
                orderDisplay.text = orderText;
                currentRequiredItem = orderText.Replace("Order: ", "").Trim();
            }
        }
    }
    
    // This is called by the Cash Register when the order is served
    public void HideOrder()
    {
        if (orderPanel != null) 
        {
            // Turn the UI box off
            orderPanel.SetActive(false);
        }
    }

    public int customersServed = 0;
public Sprite creepyPhoto; // Drag your 'CreepyAirDrop1' sprite here in the Inspector

public void CustomerLeft()
{
    customersServed++;

    // This is where the creepy photo magic happens!
    if (customersServed == 3)
    {
        TriggerHorrorEvent();
    }
}

void TriggerHorrorEvent()
{
    Debug.Log("OrderManager: Attempting to trigger AirDrop...");
    
    if (PhoneManager.Instance != null)
    {
        Debug.Log("OrderManager: PhoneManager found! Sending data...");
        PhoneManager.Instance.ReceiveAirdrop("AirDrop: 'I LIKE YOUR SHIRT.'", creepyPhoto);
    }
    else
    {
        Debug.LogError("OrderManager: PhoneManager.Instance is NULL! Is the script missing from the scene?");
    }
}}