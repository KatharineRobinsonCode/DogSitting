using UnityEngine;
using System.Collections;
using Yarn.Unity;

/// <summary>
/// Runs the pub intro as internal-monologue dialogue (Yarn Spinner).
/// Freezes the player while the node plays, then restores control when it ends.
/// </summary>
public class IntroSequence : MonoBehaviour
{
    #region Serialized Fields

    [Header("Dialogue")]
    [Tooltip("Yarn node that contains the intro monologue")]
    [SerializeField] private string introYarnNode = "PubIntro";

    #endregion

    #region Private Fields

    private DialogueRunner runner;
    private bool isComplete = false;

    #endregion

    #region Unity Lifecycle

    private void Start()
    {
        Debug.Log("[IntroSequence] Start called, isReturningFromMiniGame: " + HouseSceneState.isReturningFromMiniGame);

        if (HouseSceneState.isReturningFromMiniGame)
        {
            EndIntro();
            return;
        }

        // Freeze player during intro
        PlayerMovement pm = FindFirstObjectByType<PlayerMovement>();
        if (pm != null) pm.SetMovementEnabled(false);

        SojaExiles.MouseLook ml = FindFirstObjectByType<SojaExiles.MouseLook>();
        if (ml != null) ml.enabled = false;

        InitializeCursor();
        StartCoroutine(StartIntroDialogue());
    }

    #endregion

    #region Initialization

    private void InitializeCursor()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    #endregion

    #region Intro Dialogue

    private IEnumerator StartIntroDialogue()
    {
        yield return null;  // let the runner settle, same as the NPCs do

        runner = FindFirstObjectByType<DialogueRunner>();
        if (runner == null)
        {
            Debug.LogError("[IntroSequence] No DialogueRunner found");
            EndIntro();   // don't leave the player frozen
            yield break;
        }

        // Make sure the dialogue UI is visible
        Canvas dialogueCanvas = runner.GetComponentInChildren<Canvas>(true);
        if (dialogueCanvas != null) dialogueCanvas.gameObject.SetActive(true);

        runner.onDialogueComplete.AddListener(OnIntroDialogueComplete);
        runner.StartDialogue(introYarnNode);
    }

    private void OnIntroDialogueComplete()
    {
        runner.onDialogueComplete.RemoveListener(OnIntroDialogueComplete);
        EndIntro();
    }

    private void EndIntro()
    {
        isComplete = true;

        // Restore player control
        PlayerMovement pm = FindFirstObjectByType<PlayerMovement>();
        if (pm != null) pm.SetMovementEnabled(true);

        SojaExiles.MouseLook ml = FindFirstObjectByType<SojaExiles.MouseLook>();
        if (ml != null) ml.enabled = true;
    }

    #endregion

    #region Public Utility Methods

    /// <summary>
    /// Immediately stops the intro dialogue and gives control back to the player.
    /// </summary>
    public void ForceSkip()
    {
        StopAllCoroutines();

        if (runner != null)
        {
            runner.onDialogueComplete.RemoveListener(OnIntroDialogueComplete);
            if (runner.IsDialogueRunning) runner.Stop();
        }

        EndIntro();
    }

    /// <summary>
    /// Check if the intro sequence has finished.
    /// </summary>
    public bool IsComplete()
    {
        return isComplete;
    }

    #endregion
}