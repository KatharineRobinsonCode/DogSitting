using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class TaskManager : MonoBehaviour
{
    #region Singleton
    public static TaskManager Instance { get; private set; }
    #endregion

    #region Serialized Fields
    [Header("Task UI")]
    [SerializeField] private GameObject taskPanel;
    [SerializeField] private TextMeshProUGUI taskDisplay;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = false;
    #endregion

    #region Private Fields
    private string currentTask = "";
    private Queue<string> taskQueue = new Queue<string>();
    private List<string> completedTasks = new List<string>();
    #endregion

    #region Properties
    public string CurrentTask => currentTask;
    public bool HasActiveTask => !string.IsNullOrEmpty(currentTask);
    public int QueuedTaskCount => taskQueue.Count;
    #endregion

    #region Unity Lifecycle
    private void Awake() { InitializeSingleton(); }
    private void Start() { HideTask(); }
    #endregion

    #region Initialization
    private void InitializeSingleton()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }
    #endregion

    #region Public API - Task Management
    public void ShowTask(string taskText)
    {
        if (string.IsNullOrWhiteSpace(taskText)) return;
        currentTask = taskText;
        RefreshDisplay();
        LogDebug($"[TaskManager] New task: {taskText}");
    }

    public void UpdateTask(string taskText)
    {
        if (string.IsNullOrWhiteSpace(taskText)) return;
        currentTask = taskText;
        RefreshDisplay();
        LogDebug($"[TaskManager] Updated task: {taskText}");
    }

    public void HideTask()
    {
        if (taskPanel != null) taskPanel.SetActive(false);
        currentTask = "";
        LogDebug("[TaskManager] Task hidden");
    }

    public void CompleteTask(bool showNextTask = true)
    {
        LogDebug($"[TaskManager] Task completed: {currentTask}");

        if (!string.IsNullOrEmpty(currentTask))
            completedTasks.Add(currentTask);

        if (showNextTask && taskQueue.Count > 0)
            ShowNextQueuedTask();
        else
        {
            currentTask = "";
            RefreshDisplay();
        }
    }
    #endregion

    #region Public API - Task Queue
    public void QueueTask(string taskText)
    {
        if (string.IsNullOrWhiteSpace(taskText)) return;
        taskQueue.Enqueue(taskText);
        LogDebug($"[TaskManager] Task queued: {taskText}");
    }

    public void ShowNextQueuedTask()
    {
        if (taskQueue.Count > 0)
            ShowTask(taskQueue.Dequeue());
        else
            LogDebug("[TaskManager] No queued tasks available");
    }

    public void ClearQueue()
    {
        taskQueue.Clear();
        LogDebug("[TaskManager] Queue cleared");
    }

    public void QueueMultipleTasks(params string[] tasks)
    {
        foreach (string task in tasks) QueueTask(task);
    }

    public void SetTaskSequence(params string[] tasks)
    {
        if (tasks == null || tasks.Length == 0) return;
        ClearQueue();
        ShowTask(tasks[0]);
        for (int i = 1; i < tasks.Length; i++) QueueTask(tasks[i]);
        LogDebug($"[TaskManager] Sequence set: {tasks.Length} tasks");
    }
    #endregion

    #region Private Methods - UI
    private void RefreshDisplay()
    {
        if (taskPanel != null) taskPanel.SetActive(true);
        if (taskDisplay == null) return;

        string display = "";

        foreach (string done in completedTasks)
            display += $"<color=#888888><s>✓ {done}</s></color>\n";

        if (!string.IsNullOrEmpty(currentTask))
            display += $"<color=#FFFFFF>→ {currentTask}</color>";

        taskDisplay.text = display.TrimEnd();
    }
    #endregion

    #region Public Utility Methods
    public bool IsCurrentTask(string taskText)
    {
        return currentTask.Equals(taskText, System.StringComparison.OrdinalIgnoreCase);
    }

    public void ClearCompleted() { completedTasks.Clear(); RefreshDisplay(); }
    #endregion

    #region Debug
    private void LogDebug(string message) { if (enableDebugLogs) Debug.Log(message); }

    [ContextMenu("Log Task State")]
    public void LogTaskState()
    {
        Debug.Log($"Current: {(HasActiveTask ? currentTask : "None")}");
        Debug.Log($"Completed: {completedTasks.Count}, Queued: {taskQueue.Count}");
    }
    #endregion
}