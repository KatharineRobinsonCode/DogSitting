using UnityEngine;
using Yarn.Unity;

/// <summary>
/// Handles first-person player movement, jumping, crouching, and camera look.
/// Movement is blocked during dialogue and pause states.
/// </summary>
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(AudioSource))]
public class PlayerMovement : MonoBehaviour
{
    #region Serialized Fields
    
    [Header("Camera")]
    [Tooltip("First-person camera (player's view)")]
    [SerializeField] private Camera playerCamera;
    
    [Header("Movement Speeds")]
    [Tooltip("Normal walking speed")]
    [SerializeField] private float walkSpeed = 6f;
    
    [Tooltip("Speed when holding sprint key")]
    [SerializeField] private float runSpeed = 12f;
    
    [Tooltip("Speed while crouching")]
    [SerializeField] private float crouchSpeed = 3f;
    
    [Header("Jump Settings")]
    [Tooltip("Upward force applied when jumping")]
    [SerializeField] private float jumpPower = 7f;
    
    [Tooltip("Downward acceleration (gravity strength)")]
    [SerializeField] private float gravity = 10f;
    
    [Header("Camera Look")]
    [Tooltip("Mouse sensitivity for looking around")]
    [SerializeField] private float lookSpeed = 2f;
    
    [Tooltip("Maximum vertical look angle (prevents looking backwards)")]
    [SerializeField] private float lookXLimit = 45f;
    
    [Header("Crouch Settings")]
    [Tooltip("Character height when standing")]
    [SerializeField] private float defaultHeight = 2f;
    
    [Tooltip("Character height when crouching")]
    [SerializeField] private float crouchHeight = 1f;
    
    [Header("Audio")]
    [Tooltip("Sound played when jumping")]
    [SerializeField] private AudioClip jumpSound;
    
    #endregion
    
    #region Private Fields
    
    private CharacterController characterController;
    private AudioSource audioSource;
    private DialogueRunner dialogueRunner;
    
    private Vector3 moveDirection = Vector3.zero;
    private float rotationX = 0f;
    private bool canMove = true;
    
    // Cached speed values for crouch toggle
    private float baseWalkSpeed;
    private float baseRunSpeed;
    
    // Constants
    private const float GROUND_STICK_FORCE = -2f;
    private const KeyCode SPRINT_KEY = KeyCode.LeftShift;
    private const KeyCode CROUCH_KEY = KeyCode.C;
    private const string JUMP_INPUT = "Jump";
    private const string VERTICAL_AXIS = "Vertical";
    private const string HORIZONTAL_AXIS = "Horizontal";
    private const string MOUSE_X_AXIS = "Mouse X";
    private const string MOUSE_Y_AXIS = "Mouse Y";
    
    #endregion
    
    #region Unity Lifecycle
    
    private void Start()
    {
        InitializeComponents();
        CacheBaseValues();
        InitializeCursor();
    }
    
    private void Update()
    {
        if (!CanPlayerMove())
        {
            return;
        }
        
        HandleMovement();
        HandleCameraLook();
    }
    
    #endregion
    
    #region Initialization
    
    private void InitializeComponents()
    {
        characterController = GetComponent<CharacterController>();
        audioSource = GetComponent<AudioSource>();
        dialogueRunner = FindFirstObjectByType<DialogueRunner>();
        
        if (characterController == null)
        {
            Debug.LogError("[PlayerMovement] CharacterController component missing!");
        }
        
        if (playerCamera == null)
        {
            Debug.LogWarning("[PlayerMovement] Player camera not assigned!");
        }
    }
    
    private void CacheBaseValues()
    {
        baseWalkSpeed = walkSpeed;
        baseRunSpeed = runSpeed;
    }
    
    private void InitializeCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    
    #endregion
    
    #region Movement System
    
    private bool CanPlayerMove()
    {
        // Block movement when paused
        if (IsPaused())
        {
            return false;
        }
        
        // Block movement during dialogue
        if (IsDialogueRunning())
        {
            return false;
        }
        
        return canMove;
    }
    
    private bool IsPaused()
    {
        return PauseManager.Instance != null && PauseManager.Instance.IsPaused();
    }
    
    private bool IsDialogueRunning()
    {
        return dialogueRunner != null && dialogueRunner.IsDialogueRunning;
    }
    
    private void HandleMovement()
    {
        UpdateMovementDirection();
        ApplyGravityAndJump();
        HandleCrouching();
        ApplyMovement();
    }
    
    private void UpdateMovementDirection()
    {
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);
        
        bool isSprinting = Input.GetKey(SPRINT_KEY);
        float currentSpeed = isSprinting ? runSpeed : walkSpeed;
        
        float verticalInput = Input.GetAxis(VERTICAL_AXIS);
        float horizontalInput = Input.GetAxis(HORIZONTAL_AXIS);
        
        // Store Y velocity before updating horizontal movement
        float verticalVelocity = moveDirection.y;
        
        // Calculate horizontal movement
        moveDirection = (forward * verticalInput + right * horizontalInput) * currentSpeed;
        
        // Restore Y velocity (don't let horizontal movement affect falling)
        moveDirection.y = verticalVelocity;
    }
    
    private void ApplyGravityAndJump()
    {
        if (characterController.isGrounded)
        {
            HandleGroundedState();
        }
        else
        {
            HandleAirborneState();
        }
    }
    
    private void HandleGroundedState()
    {
        // Slight downward force keeps player grounded on slopes/stairs
        moveDirection.y = GROUND_STICK_FORCE;
        
        if (Input.GetButton(JUMP_INPUT))
        {
            Jump();
        }
    }
    
    private void HandleAirborneState()
    {
        // Apply gravity while in air
        moveDirection.y -= gravity * Time.deltaTime;
    }
    
    private void Jump()
    {
        moveDirection.y = jumpPower;
        PlayJumpSound();
    }
    
    private void PlayJumpSound()
    {
        if (audioSource != null && jumpSound != null)
        {
            audioSource.PlayOneShot(jumpSound);
        }
    }
    
    private void HandleCrouching()
    {
        if (Input.GetKey(CROUCH_KEY))
        {
            EnableCrouch();
        }
        else
        {
            DisableCrouch();
        }
    }
    
    private void EnableCrouch()
    {
        characterController.height = crouchHeight;
        walkSpeed = crouchSpeed;
        runSpeed = crouchSpeed;
    }
    
    private void DisableCrouch()
    {
        characterController.height = defaultHeight;
        walkSpeed = baseWalkSpeed;
        runSpeed = baseRunSpeed;
    }
    
    private void ApplyMovement()
    {
        characterController.Move(moveDirection * Time.deltaTime);
    }
    
    #endregion
    
    #region Camera Look System
    
    private void HandleCameraLook()
    {
        UpdateVerticalLook();
        UpdateHorizontalLook();
    }
    
    private void UpdateVerticalLook()
    {
        if (playerCamera == null) return;
        
        float mouseY = Input.GetAxis(MOUSE_Y_AXIS);
        rotationX += -mouseY * lookSpeed;
        rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
        
        playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0f, 0f);
    }
    
    private void UpdateHorizontalLook()
    {
        float mouseX = Input.GetAxis(MOUSE_X_AXIS);
        transform.rotation *= Quaternion.Euler(0f, mouseX * lookSpeed, 0f);
    }
    
    #endregion
    
    #region Public API
    
    /// <summary>
    /// Enables or disables player movement (useful for cutscenes)
    /// </summary>
    public void SetMovementEnabled(bool enabled)
    {
        canMove = enabled;
    }
    
    /// <summary>
    /// Returns true if player is currently grounded
    /// </summary>
    public bool IsGrounded()
    {
        return characterController != null && characterController.isGrounded;
    }
    
    /// <summary>
    /// Returns current movement velocity
    /// </summary>
    public Vector3 GetVelocity()
    {
        return characterController != null ? characterController.velocity : Vector3.zero;
    }
    
    #endregion
}