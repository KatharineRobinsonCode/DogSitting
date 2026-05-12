using UnityEngine;

public static class HouseSceneState
{
    public static bool isReturningFromMiniGame = false;
    
    // Player
    public static Vector3 playerPosition;
    public static Quaternion playerRotation;
    
    // Task
    public static string savedTask;
    public static string[] savedTaskQueue;
    
    // Audio flags
    public static bool skipExteriorAudio = false;
    public static bool skipBarkTrigger = false;
    
    public static void SaveState(Vector3 position, Quaternion rotation)
    {
        isReturningFromMiniGame = true;
        playerPosition = position;
        playerRotation = rotation;
        skipExteriorAudio = true;
        skipBarkTrigger = true;
        
        // Save current task state
        if (TaskManager.Instance != null)
        {
            savedTask = TaskManager.Instance.CurrentTask;
        }
    }
    
    public static void Clear()
    {
        isReturningFromMiniGame = false;
        skipExteriorAudio = false;
        skipBarkTrigger = false;
        savedTask = null;
        savedTaskQueue = null;
    }
}