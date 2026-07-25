using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;

namespace SojaExiles
{
    public class MouseLook : MonoBehaviour
    {
        public float mouseXSensitivity = 100f;
        public Transform playerBody;

        float xRotation = 0f;
        private DialogueRunner dialogueRunner;

       void Start()
{
    Cursor.lockState = CursorLockMode.Locked;
    dialogueRunner = FindFirstObjectByType<DialogueRunner>();

    // Load saved sensitivity
    mouseXSensitivity = PlayerPrefs.GetFloat("MouseSensitivity", mouseXSensitivity);
}

        void Update()
        {
            // Block camera look during dialogue
            if (dialogueRunner != null && dialogueRunner.IsDialogueRunning)
                return;

            float mouseX = Input.GetAxis("Mouse X") * mouseXSensitivity * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * mouseXSensitivity * Time.deltaTime;

            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);

            transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
            playerBody.Rotate(Vector3.up * mouseX);
        }
    }
}