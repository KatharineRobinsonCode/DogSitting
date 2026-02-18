using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;

// This line ensures that the object MUST have a CharacterController component attached
[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public Camera playerCamera;      // The "Eyes" of the player
    public float walkSpeed = 6f;     // Normal walking speed
    public float runSpeed = 12f;      // Speed when holding Shift
    public float jumpPower = 7f;      // How high the player leaps
    public float gravity = 10f;       // How fast the player falls back down
    public float lookSpeed = 2f;      // Sensitivity of the mouse
    public float lookXLimit = 45f;    // Limits how far you can look up or down (prevents neck-snapping!)
    
    [Header("Crouch Settings")]
    public float defaultHeight = 2f;  // Normal standing height
    public float crouchHeight = 1f;   // Height when ducking
    public float crouchSpeed = 3f;    // Speed while ducking

    // Internal variables to keep track of the player's state
    private Vector3 moveDirection = Vector3.zero; // The direction the player is currently moving
    private float rotationX = 0;                  // Current vertical head tilt
    private CharacterController characterController; // The movement component
    
    // --- UPDATED: Added a variable to store the dialogue runner ---
    private DialogueRunner dialogueRunner;

    private bool canMove = true; // Useful if you want to freeze the player during a cutscene

    [Header("Audio")]
    public AudioClip jumpSound;   // The sound file for jumping
    private AudioSource audioSource; // The component that plays the sound

    void Start()
    {
        // Link up the components on the Player object
        characterController = GetComponent<CharacterController>();
        audioSource = GetComponent<AudioSource>();

        // --- UPDATED: Find the DialogueRunner only once when the game starts ---
        // This is much faster for the computer than looking for it every frame.
        dialogueRunner = FindFirstObjectByType<DialogueRunner>();

        // Lock the mouse cursor to the middle of the screen and hide it
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
    // Don't allow movement when paused
    if (PauseManager.Instance != null && PauseManager.Instance.IsPaused())
    {
        return; // Exit early, no movement allowed
    }
       
        // --- UPDATED: Safe check for dialogue ---
        // We check "dialogueRunner != null" first. If the DialogueRunner is missing, 
        // it skips the second part instead of crashing your game.
        if (dialogueRunner != null && dialogueRunner.IsDialogueRunning)
        {
            // If dialogue is open, stop movement and exit Update
            return; 
        }

        // 1. HORIZONTAL MOVEMENT
        // Figure out which way is "Forward" and "Right" based on where the player is facing
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        // Check if we are holding Left Shift to run
        bool isRunning = Input.GetKey(KeyCode.LeftShift);

        // Calculate speed based on WASD input. 
        // If canMove is false, speed is 0. If isRunning is true, use runSpeed.
        float curSpeedX = canMove ? (isRunning ? runSpeed : walkSpeed) * Input.GetAxis("Vertical") : 0;
        float curSpeedY = canMove ? (isRunning ? runSpeed : walkSpeed) * Input.GetAxis("Horizontal") : 0;
        
        // Store our current downward (gravity) speed so we don't lose it when we move sideways
        float movementDirectionY = moveDirection.y;
        
        // Combine forward/backward and left/right movement into one direction
        moveDirection = (forward * curSpeedX) + (right * curSpeedY);

        // 2. GRAVITY & JUMPING
        // isGrounded is a special check that knows if your feet are touching the floor
        if (characterController.isGrounded)
        {
            // We set Y to -2 instead of 0 so the player stays "snapped" to the floor on stairs
            moveDirection.y = -2f; 

            // If the jump button (Space) is pressed
            if (Input.GetButton("Jump") && canMove)
            {
                moveDirection.y = jumpPower; // Launch the player upward
                if (audioSource != null && jumpSound != null) audioSource.PlayOneShot(jumpSound);
            }
        }
        else
        {
            // If we are in the air, subtract gravity over time to fall
            moveDirection.y = movementDirectionY - (gravity * Time.deltaTime);
        }

        // 3. CROUCH LOGIC (Holding 'C')
        if (Input.GetKey(KeyCode.C) && canMove)
        {
            characterController.height = crouchHeight; // Make the character shorter
            walkSpeed = crouchSpeed; // Slow down while crouching
            runSpeed = crouchSpeed;
        }
        else
        {
            // Reset to normal standing height and speed
            characterController.height = defaultHeight;
            walkSpeed = 6f;
            runSpeed = 12f;
        }

        // 4. THE MOVE COMMAND: This actually applies all the math above to the character
        characterController.Move(moveDirection * Time.deltaTime);

        // 5. CAMERA LOOK: Rotate the head (up/down) and the body (left/right)
        if (canMove)
        {
            // Rotate the head up/down based on Mouse Y
            rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
            // Clamp ensures you can't look so far back that you see inside your own neck
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
            playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);

            // Rotate the whole player body left/right based on Mouse X
            transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);
        }
    }
}