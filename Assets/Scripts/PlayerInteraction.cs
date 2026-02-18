using UnityEngine;
using TMPro;
using Yarn.Unity;

public class PlayerInteraction : MonoBehaviour
{
    [Header("UI References")]
    public float interactDistance = 5f;
    public GameObject promptObject; // DRAG THE "InteractionPrompt" CHILD HERE
    
    public GameObject interactionPromptCanvas; // ADD THIS - drag InteractionPromptCanvas here

    public LayerMask excludeLayers;
    private TextMeshProUGUI actualText;

    [Header("Hand Settings")]
    public Transform holdPoint;
    public Vector3 holdPosition = new Vector3(0.4f, -0.4f, 0.7f);
    public Vector3 holdRotation = new Vector3(30, 0, 0);

    [Header("State")]
    public GameObject currentHeldItem;
    private int originalLayer;
    private DialogueRunner dialogueRunner;

    [Header("Crosshair Settings")]
    public UnityEngine.UI.Image crosshairImage; // DRAG THE "Crosshair" CHILD HERE
    public Color normalColor = Color.white;
    public Color interactColor = Color.yellow;
    public float interactScale = 1.2f;

  void Awake()
    {
 if (interactionPromptCanvas != null)
    {
        interactionPromptCanvas.SetActive(true);
        Debug.Log("AWAKE: Forced InteractionPromptCanvas ON");
    }
    }

    void Start()
    {
        dialogueRunner = FindFirstObjectByType<DialogueRunner>();
        if (promptObject != null)
        {
            actualText = promptObject.GetComponentInChildren<TextMeshProUGUI>();
            // Ensure the prompt itself starts hidden
            promptObject.SetActive(false);
        }
        
        // Final desperate attempt: if the canvas is STILL off, force it.
        if (crosshairImage != null && crosshairImage.canvas != null)
        {
            crosshairImage.canvas.gameObject.SetActive(true);
            Debug.Log("Forcing Canvas ON in Start");
        }
            if (interactionPromptCanvas != null)
    {
        interactionPromptCanvas.SetActive(true);
        Debug.Log("START: Forced InteractionPromptCanvas ON");
    }
    }

    void Update()
    {
        if (Camera.main == null) return;

        // 1. DIALOGUE CHECK
        if (dialogueRunner != null && dialogueRunner.IsDialogueRunning)
        {
            UpdateUI(false, "");
            return;
        }

        // 2. RAYCAST LOGIC
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
        RaycastHit hit;
        bool isLookingAtInteractable = false;
        string promptMessage = "";

        // Note: Check if interactDistance is set to 5 in inspector!
        if (Physics.Raycast(ray, out hit, interactDistance, ~excludeLayers))
        {
            IInteractable interactable = hit.collider.GetComponentInParent<IInteractable>();
            Cup cup = hit.collider.GetComponentInParent<Cup>();
            DrinkMachine machine = hit.collider.GetComponentInParent<DrinkMachine>();
            Register register = hit.collider.GetComponentInParent<Register>();
            Trash bin = hit.collider.GetComponentInParent<Trash>();

            if (interactable != null || cup != null || machine != null || register != null || bin != null)
            {
                if (interactable != null) promptMessage = interactable.GetInteractionPrompt();
                else if (machine != null) promptMessage = "Press E to use " + machine.gameObject.name;
                else if (register != null) promptMessage = "Press E to use Register";
                else if (bin != null) promptMessage = "Press E to use Bin";
                else if (cup != null && currentHeldItem == null) promptMessage = "Press E to pick up Cup";

                if (!string.IsNullOrEmpty(promptMessage)) isLookingAtInteractable = true;

                if (Input.GetKeyDown(KeyCode.E) && isLookingAtInteractable)
                {
                    if (interactable != null) interactable.Interact(this);
                    else if (machine != null) machine.Interact(this);
                    else if (register != null) register.Interact(this);
                    else if (bin != null) bin.Interact(this);
                    else if (cup != null && currentHeldItem == null) PickUp(cup.gameObject);
                }
            }
        }

        UpdateUI(isLookingAtInteractable, promptMessage);
    }

    void UpdateUI(bool looking, string msg)
    {
         // THE GUARDIAN: Force the InteractionPromptCanvas to stay on
    if (interactionPromptCanvas != null && !interactionPromptCanvas.activeSelf)
    {
        interactionPromptCanvas.SetActive(true);
        Debug.Log("<color=cyan>Guardian:</color> Forced InteractionPromptCanvas back on!");
    }

        // 1. Handle Prompt Text (The child)
        if (promptObject != null)
        {
            promptObject.SetActive(looking);
            if (looking && actualText != null) actualText.text = msg;
        }

        // 2. Handle Crosshair (Always on, just changes color/scale)
        if (crosshairImage != null)
        {
            crosshairImage.color = looking ? interactColor : normalColor;
            
            float targetScale = looking ? interactScale : 1.0f;
            if (looking) // Pulse effect
            {
                targetScale *= (1.0f + Mathf.Sin(Time.time * 10f) * 0.1f);
            }
            crosshairImage.transform.localScale = Vector3.one * targetScale;
        }
    }

    void PickUp(GameObject item)
    {
        currentHeldItem = item;
        originalLayer = item.layer;
        int heldLayer = LayerMask.NameToLayer("HeldItems");
        if (heldLayer != -1) SetLayerRecursive(item, heldLayer);

        if (item.TryGetComponent(out Rigidbody rb)) rb.isKinematic = true;
        if (item.TryGetComponent(out Collider col)) col.isTrigger = true;
        
        item.transform.SetParent(holdPoint);
        item.transform.localPosition = holdPosition;
        item.transform.localRotation = Quaternion.Euler(holdRotation);
    }

    void SetLayerRecursive(GameObject obj, int newLayer)
    {
        obj.layer = newLayer;
        foreach (Transform child in obj.transform) SetLayerRecursive(child.gameObject, newLayer);
    }
}