using UnityEngine;
using TMPro;
using Yarn.Unity;

/// <summary>
/// Handles player interaction with objects in the world via raycasting.
/// Manages UI feedback (crosshair and prompts) and item pickup/holding.
/// </summary>
public class PlayerInteraction : MonoBehaviour
{
    #region Serialized Fields
    
    [Header("Interaction Settings")]
    [Tooltip("Maximum distance to interact with objects")]
    [SerializeField] private float interactDistance = 5f;
    
    [Tooltip("Layers to exclude from raycasting")]
    [SerializeField] private LayerMask excludeLayers;
    
    [Header("UI References")]
    [Tooltip("Parent canvas that contains all interaction UI")]
    [SerializeField] private GameObject interactionPromptCanvas;
    
    [Tooltip("Text prompt GameObject (child of canvas)")]
    [SerializeField] private GameObject promptObject;
    
    [Tooltip("Crosshair image (child of canvas)")]
    [SerializeField] private UnityEngine.UI.Image crosshairImage;
    
    [Header("Crosshair Settings")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color interactColor = Color.yellow;
    
    [Tooltip("Scale multiplier when looking at interactable")]
    [SerializeField] private float interactScale = 1.2f;
    
    [Tooltip("Pulse animation speed")]
    [SerializeField] private float pulseSpeed = 10f;
    
    [Tooltip("Pulse animation intensity")]
    [SerializeField] private float pulseIntensity = 0.1f;
    
    [Header("Item Holding")]
    [Tooltip("Transform where held items will be positioned")]
    [SerializeField] private Transform holdPoint;
    
    [SerializeField] private Vector3 holdPosition = new Vector3(0.4f, -0.4f, 0.7f);
    [SerializeField] private Vector3 holdRotation = new Vector3(30, 0, 0);
    
    [Header("Debug")]
    [Tooltip("Currently held item (read-only)")]
    [SerializeField] private GameObject currentHeldItem;
    
    #endregion
    
    #region Private Fields
    
    private TextMeshProUGUI promptText;
    private DialogueRunner dialogueRunner;
    private int originalItemLayer;
    
    // Constants
    private const string HELD_ITEMS_LAYER = "HeldItems";
    private const KeyCode INTERACT_KEY = KeyCode.E;
    
    #endregion
    
    #region Properties
    
    /// <summary>
    /// Public read-only access to currently held item
    /// </summary>
    public GameObject CurrentHeldItem => currentHeldItem;
    
    #endregion
    
    #region Unity Lifecycle
    
    private void Awake()
    {
        EnsureCanvasActive();
    }
    
    private void Start()
    {
        InitializeComponents();
        EnsureCanvasActive();
    }
    
    private void Update()
    {
        if (!CanInteract())
        {
            UpdateUI(false, string.Empty);
            return;
        }
        
        CheckForInteractables();
    }
    
    #endregion
    
    #region Initialization
    
    private void InitializeComponents()
    {
        dialogueRunner = FindFirstObjectByType<DialogueRunner>();
        
        if (promptObject != null)
        {
            promptText = promptObject.GetComponentInChildren<TextMeshProUGUI>();
            promptObject.SetActive(false);
        }
        
        if (crosshairImage != null && crosshairImage.canvas != null)
        {
            crosshairImage.canvas.gameObject.SetActive(true);
        }
    }
    
    private void EnsureCanvasActive()
    {
        if (interactionPromptCanvas != null)
        {
            interactionPromptCanvas.SetActive(true);
        }
    }
    
    #endregion
    
    #region Interaction System
    
    private bool CanInteract()
    {
        // Ensure canvas stays active (guardian check)
        EnsureCanvasActive();
        
        if (Camera.main == null)
        {
            return false;
        }
        
        // Block interaction during dialogue
        if (dialogueRunner != null && dialogueRunner.IsDialogueRunning)
        {
            return false;
        }
        
        return true;
    }
    
    private void CheckForInteractables()
    {
        Ray ray = GetCenterScreenRay();
        
        if (TryRaycastInteractable(ray, out RaycastHit hit, out string promptMessage))
        {
            UpdateUI(true, promptMessage);
            
            if (Input.GetKeyDown(INTERACT_KEY))
            {
                HandleInteraction(hit);
            }
        }
        else
        {
            UpdateUI(false, string.Empty);
        }
    }
    
    private Ray GetCenterScreenRay()
    {
        Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f);
        return Camera.main.ScreenPointToRay(screenCenter);
    }
    
    private bool TryRaycastInteractable(Ray ray, out RaycastHit hit, out string promptMessage)
    {
        promptMessage = string.Empty;
        
        if (!Physics.Raycast(ray, out hit, interactDistance, ~excludeLayers))
        {
            return false;
        }
        
        // Check for all interactable types
        IInteractable interactable = hit.collider.GetComponentInParent<IInteractable>();
        if (interactable != null)
        {
            promptMessage = interactable.GetInteractionPrompt();
            return !string.IsNullOrEmpty(promptMessage);
        }
        
        DrinkMachine machine = hit.collider.GetComponentInParent<DrinkMachine>();
        if (machine != null)
        {
            promptMessage = $"Press E to use {machine.gameObject.name}";
            return true;
        }
        
        Register register = hit.collider.GetComponentInParent<Register>();
        if (register != null)
        {
            promptMessage = "Press E to use Register";
            return true;
        }
        
        Trash bin = hit.collider.GetComponentInParent<Trash>();
        if (bin != null)
        {
            promptMessage = "Press E to use Bin";
            return true;
        }
        
        Cup cup = hit.collider.GetComponentInParent<Cup>();
        if (cup != null && currentHeldItem == null)
        {
            promptMessage = "Press E to pick up Cup";
            return true;
        }
        
        return false;
    }
    
    private void HandleInteraction(RaycastHit hit)
    {
        // Try each interactable type in priority order
        IInteractable interactable = hit.collider.GetComponentInParent<IInteractable>();
        if (interactable != null)
        {
            interactable.Interact(this);
            return;
        }
        
        DrinkMachine machine = hit.collider.GetComponentInParent<DrinkMachine>();
        if (machine != null)
        {
            machine.Interact(this);
            return;
        }
        
        Register register = hit.collider.GetComponentInParent<Register>();
        if (register != null)
        {
            register.Interact(this);
            return;
        }
        
        Trash bin = hit.collider.GetComponentInParent<Trash>();
        if (bin != null)
        {
            bin.Interact(this);
            return;
        }
        
        Cup cup = hit.collider.GetComponentInParent<Cup>();
        if (cup != null && currentHeldItem == null)
        {
            PickUpItem(cup.gameObject);
            return;
        }
    }
    
    #endregion
    
    #region UI Updates
    
    private void UpdateUI(bool isLookingAtInteractable, string message)
    {
        UpdatePromptText(isLookingAtInteractable, message);
        UpdateCrosshair(isLookingAtInteractable);
    }
    
    private void UpdatePromptText(bool shouldShow, string message)
    {
        if (promptObject == null) return;
        
        promptObject.SetActive(shouldShow);
        
        if (shouldShow && promptText != null)
        {
            promptText.text = message;
        }
    }
    
    private void UpdateCrosshair(bool isInteractable)
    {
        if (crosshairImage == null) return;
        
        UpdateCrosshairColor(isInteractable);
        UpdateCrosshairScale(isInteractable);
    }
    
    private void UpdateCrosshairColor(bool isInteractable)
    {
        crosshairImage.color = isInteractable ? interactColor : normalColor;
    }
    
    private void UpdateCrosshairScale(bool isInteractable)
    {
        float targetScale = CalculateCrosshairScale(isInteractable);
        crosshairImage.transform.localScale = Vector3.one * targetScale;
    }
    
    private float CalculateCrosshairScale(bool isInteractable)
    {
        if (!isInteractable)
        {
            return 1.0f;
        }
        
        // Add pulse animation when looking at interactable
        float pulseOffset = Mathf.Sin(Time.time * pulseSpeed) * pulseIntensity;
        return interactScale * (1.0f + pulseOffset);
    }
    
    #endregion
    
    #region Item Pickup & Holding
    
    private void PickUpItem(GameObject item)
    {
        currentHeldItem = item;
        
        // Store original layer for restoration
        originalItemLayer = item.layer;
        
        ConfigureItemPhysics(item);
        SetItemLayer(item);
        AttachItemToHoldPoint(item);
    }
    
    private void ConfigureItemPhysics(GameObject item)
    {
        if (item.TryGetComponent(out Rigidbody rb))
        {
            rb.isKinematic = true;
        }
        
        if (item.TryGetComponent(out Collider col))
        {
            col.isTrigger = true;
        }
    }
    
    private void SetItemLayer(GameObject item)
    {
        int heldLayer = LayerMask.NameToLayer(HELD_ITEMS_LAYER);
        
        if (heldLayer != -1)
        {
            SetLayerRecursive(item, heldLayer);
        }
        else
        {
            Debug.LogWarning($"Layer '{HELD_ITEMS_LAYER}' not found. Item may still be raycast-detectable.");
        }
    }
    
    private void AttachItemToHoldPoint(GameObject item)
    {
        item.transform.SetParent(holdPoint);
        item.transform.localPosition = holdPosition;
        item.transform.localRotation = Quaternion.Euler(holdRotation);
    }
    
    private void SetLayerRecursive(GameObject obj, int newLayer)
    {
        obj.layer = newLayer;
        
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursive(child.gameObject, newLayer);
        }
    }
    
    /// <summary>
    /// Drops the currently held item (call from other scripts if needed)
    /// </summary>
    public void DropHeldItem()
    {
        if (currentHeldItem == null) return;
        
        // Restore physics
        if (currentHeldItem.TryGetComponent(out Rigidbody rb))
        {
            rb.isKinematic = false;
        }
        
        if (currentHeldItem.TryGetComponent(out Collider col))
        {
            col.isTrigger = false;
        }
        
        // Restore original layer
        SetLayerRecursive(currentHeldItem, originalItemLayer);
        
        // Detach from hand
        currentHeldItem.transform.SetParent(null);
        
        currentHeldItem = null;
    }
    
    #endregion
}