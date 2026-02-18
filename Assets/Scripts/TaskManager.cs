using UnityEngine;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Universal task/objective manager for displaying current player objectives.
/// Works across all scenes to show what the player should do next.
/// </summary>
public class TaskManager : MonoBehaviour
{
    #region Singleton
    
    public static TaskManager Instance { get; private set; }
    
    #endregion
    
    #region Serialized Fields
    
    [Header("Task UI")]
    [Tooltip("Background panel containing task information")]
    [SerializeField] private GameObject taskPanel;
    
    [Tooltip("Text display showing current task/objective")]
    [SerializeField] private TextMeshProUGUI taskDisplay;
    
    [Header("Task Prefix")]
    [Tooltip("Prefix shown before each task (e.g., 'Objective:', 'Task:')")]
    [SerializeField] private string taskPrefix = "Task: ";
    
    [Header("Debug")]
    [Tooltip("Enable detailed task logging")]
    [SerializeField] private bool enableDebugLogs = false;
    
    #endregion
    
    #region Private Fields
    
    private string currentTask = "";
    private Queue<string> taskQueue = new Queue<string>();
    
    #endregion
    
    #region Properties
    
    /// <summary>
    /// Returns the current active task text (without prefix).
    /// </summary>
    public string CurrentTask => currentTask;
    
    /// <summary>
    /// Returns true if there's an active task displayed.
    /// </summary>
    public bool HasActiveTask => !string.IsNullOrEmpty(currentTask);
    
    /// <summary>
    /// Returns the number of queued tasks waiting to be shown.
    /// </summary>
    public int QueuedTaskCount => taskQueue.Count;
    
    #endregion
    
    #region Unity Lifecycle
    
    private void Awake()
    {
        InitializeSingleton();
    }
    
    private void Start()
    {
        InitializeUI();
    }
    
    #endregion
    
    #region Initialization
    
    private void InitializeSingleton()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        
        // Make persistent across scenes (optional - remove if you want per-scene instances)
        DontDestroyOnLoad(gameObject);
    }
    
    private void InitializeUI()
    {
        HideTask();
    }
    
    #endregion
    
    #region Public API - Task Management
    
    /// <summary>
    /// Shows a new task to the player, replacing any current task.
    /// </summary>
    /// <param name="taskText">Task description (e.g., "Go behind counter")</param>
    public void ShowTask(string taskText)
    {
        if (string.IsNullOrWhiteSpace(taskText))
        {
            LogDebug("[TaskManager] Attempted to show empty task");
            return;
        }
        
        currentTask = taskText;
        DisplayTaskUI(FormatTaskText(taskText));
        
        LogDebug($"[TaskManager] New task: {taskText}");
    }
    
    /// <summary>
    /// Updates the current task text without hiding/showing animation.
    /// </summary>
    /// <param name="taskText">Updated task description</param>
    public void UpdateTask(string taskText)
    {
        if (string.IsNullOrWhiteSpace(taskText))
        {
            LogDebug("[TaskManager] Attempted to update to empty task");
            return;
        }
        
        currentTask = taskText;
        
        if (taskDisplay != null)
        {
            taskDisplay.text = FormatTaskText(taskText);
        }
        
        LogDebug($"[TaskManager] Updated task: {taskText}");
    }
    
    /// <summary>
    /// Hides the current task UI.
    /// </summary>
    public void HideTask()
    {
        if (taskPanel != null)
        {
            taskPanel.SetActive(false);
        }
        
        currentTask = "";
        
        LogDebug("[TaskManager] Task hidden");
    }
    
    /// <summary>
    /// Completes the current task and optionally shows the next one.
    /// </summary>
    /// <param name="showNextTask">If true and there are queued tasks, shows the next one</param>
    public void CompleteTask(bool showNextTask = true)
    {
        LogDebug($"[TaskManager] Task completed: {currentTask}");
        
        if (showNextTask && taskQueue.Count > 0)
        {
            ShowNextQueuedTask();
        }
        else
        {
            HideTask();
        }
    }
    
    #endregion
    
    #region Public API - Task Queue
    
    /// <summary>
    /// Adds a task to the queue to be shown later.
    /// </summary>
    /// <param name="taskText">Task description</param>
    public void QueueTask(string taskText)
    {
        if (string.IsNullOrWhiteSpace(taskText))
        {
            return;
        }
        
        taskQueue.Enqueue(taskText);
        LogDebug($"[TaskManager] Task queued: {taskText} (Queue size: {taskQueue.Count})");
    }
    
    /// <summary>
    /// Shows the next task from the queue if available.
    /// </summary>
    public void ShowNextQueuedTask()
    {
        if (taskQueue.Count > 0)
        {
            string nextTask = taskQueue.Dequeue();
            ShowTask(nextTask);
        }
        else
        {
            LogDebug("[TaskManager] No queued tasks available");
        }
    }
    
    /// <summary>
    /// Clears all queued tasks without showing them.
    /// </summary>
    public void ClearQueue()
    {
        int count = taskQueue.Count;
        taskQueue.Clear();
        LogDebug($"[TaskManager] Cleared {count} queued tasks");
    }
    
    #endregion
    
    #region Public API - Batch Tasks
    
    /// <summary>
    /// Queues multiple tasks at once.
    /// </summary>
    /// <param name="tasks">Array of task descriptions</param>
    public void QueueMultipleTasks(params string[] tasks)
    {
        foreach (string task in tasks)
        {
            QueueTask(task);
        }
    }
    
    /// <summary>
    /// Shows the first task and queues the rest.
    /// </summary>
    /// <param name="tasks">Array of task descriptions in order</param>
    public void SetTaskSequence(params string[] tasks)
    {
        if (tasks == null || tasks.Length == 0)
        {
            return;
        }
        
        // Clear existing queue
        ClearQueue();
        
        // Show first task
        ShowTask(tasks[0]);
        
        // Queue remaining tasks
        for (int i = 1; i < tasks.Length; i++)
        {
            QueueTask(tasks[i]);
        }
        
        LogDebug($"[TaskManager] Task sequence set: {tasks.Length} tasks");
    }
    
    #endregion
    
    #region Private Methods - UI
    
    private void DisplayTaskUI(string formattedText)
    {
        if (taskPanel != null)
        {
            taskPanel.SetActive(true);
        }
        
        if (taskDisplay != null)
        {
            taskDisplay.text = formattedText;
        }
    }
    
    private string FormatTaskText(string taskText)
    {
        return taskPrefix + taskText;
    }
    
    #endregion
    
    #region Public Utility Methods
    
    /// <summary>
    /// Changes the task prefix (e.g., from "Task:" to "Objective:").
    /// </summary>
    public void SetTaskPrefix(string prefix)
    {
        taskPrefix = prefix;
        
        // Update current task display if active
        if (HasActiveTask)
        {
            UpdateTask(currentTask);
        }
    }
    
    /// <summary>
    /// Checks if a specific task is currently active.
    /// </summary>
    public bool IsCurrentTask(string taskText)
    {
        return currentTask.Equals(taskText, System.StringComparison.OrdinalIgnoreCase);
    }
    
    #endregion
    
    #region Debug Helpers
    
    private void LogDebug(string message)
    {
        if (enableDebugLogs)
        {
            Debug.Log(message);
        }
    }
    
    /// <summary>
    /// Logs current task state to console.
    /// </summary>
    [ContextMenu("Log Task State")]
    public void LogTaskState()
    {
        Debug.Log("=== TASK MANAGER STATE ===");
        Debug.Log($"Current Task: {(HasActiveTask ? currentTask : "None")}");
        Debug.Log($"Queued Tasks: {taskQueue.Count}");
        
        if (taskQueue.Count > 0)
        {
            Debug.Log("Queue contents:");
            int index = 1;
            foreach (string task in taskQueue)
            {
                Debug.Log($"  {index}. {task}");
                index++;
            }
        }
        
        Debug.Log("========================");
    }
    
    #endregion
}