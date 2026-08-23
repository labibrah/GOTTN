using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

/// <summary>
/// Central telemetry singleton for the CHI study.
/// Owns per-scene accumulators (idle time, sprint time), a position sample
/// buffer (for path efficiency), a step counter, and generic discrete events
/// (items collected, etc). Flushes to disk as CSV on scene exit and on quit.
///
/// Usage:
///   AnalyticsLogger.Instance.IncrementCounter("steps");
///   AnalyticsLogger.Instance.LogPosition(myRigidbody.position);
///   AnalyticsLogger.Instance.AddIdleTime(Time.deltaTime);
///   AnalyticsLogger.Instance.AddSprintTime(Time.deltaTime);
///   AnalyticsLogger.Instance.LogEvent("item_collected", "item=IceWine");
///   AnalyticsLogger.Instance.SceneEnter("VillageScene");
///   AnalyticsLogger.Instance.SceneExit(); // call before loading next scene
/// </summary>
public class AnalyticsLogger : MonoBehaviour
{
    public static AnalyticsLogger Instance { get; private set; }

    [Header("Participant")]
    [Tooltip("Set this at the start of a session, e.g. from a login/consent screen.")]
    public string participantId = "unassigned";

    [Header("Output")]
    [Tooltip("Relative to Application.persistentDataPath.")]
    public string outputFolder = "AnalyticsLogs";

    // --- Internal state ---
    private string currentScene = "none";
    private float sceneEnterTime;
    private float idleAccumulator;
    private float sprintAccumulator;
    private int stepCount;
    private readonly List<Vector2> positionSamples = new List<Vector2>();

    // Rows written across the whole session, flushed at OnApplicationQuit
    // in addition to per-scene flush, so nothing is lost mid-scene on crash.
    private readonly List<string> sceneSummaryRows = new List<string>();
    private readonly List<string> eventRows = new List<string>();
    private readonly List<string> positionRows = new List<string>();

    private string sessionId;
    private bool sceneActive;

    private const string SceneSummaryHeader =
        "participant_id,session_id,scene,enter_time,exit_time,duration_sec,steps,idle_sec,sprint_sec,path_length,shortest_path,path_efficiency";
    private const string EventHeader =
        "participant_id,session_id,scene,timestamp,event_type,payload";
    private const string PositionHeader =
        "participant_id,session_id,scene,timestamp,x,y";

    private bool loggingEnabled = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        sessionId = Guid.NewGuid().ToString("N").Substring(0, 8);
    }

    void OnEnable()
    {
        // Auto-enter on every scene load, including ones not triggered
        // through SceneTransition (e.g. respawns, debug loads). SceneExit()
        // is still called manually at the point of departure (see
        // SceneTransition.OnTriggerEnter2D) since that's the only place
        // that knows the transition is intentional and pre-fade.
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        SceneEnter(scene.name);
    }

    // NOTE: position sampling is NOT done here on a timer. PlayerExploring
    // owns its own SamplePosition() coroutine and calls LogPosition()
    // directly, since it already has a Rigidbody2D reference and doesn't
    // need a per-frame tag lookup. Sampling here too would double-log
    // every sample — keep it single-sourced from the player script.

    // --- Public API ---

    public void SceneEnter(string sceneName)
    {
        if (!loggingEnabled) return;

        // If a previous scene wasn't explicitly exited (e.g. abrupt scene
        // load), flush it first so data isn't silently dropped.
        if (sceneActive)
            SceneExit();

        currentScene = sceneName;
        sceneEnterTime = Time.time;
        idleAccumulator = 0f;
        sprintAccumulator = 0f;
        stepCount = 0;
        positionSamples.Clear();
        sceneActive = true;
    }

    public void SceneExit()
    {
        if (!loggingEnabled || !sceneActive) return;
        if (!sceneActive) return;

        float duration = Time.time - sceneEnterTime;
        float pathLength = ComputePathLength();
        float shortestPath = ComputeShortestPath();
        float efficiency = shortestPath > 0f ? pathLength / shortestPath : 0f;

        sceneSummaryRows.Add(string.Join(",", new[]
        {
            participantId,
            sessionId,
            currentScene,
            sceneEnterTime.ToString("F2"),
            Time.time.ToString("F2"),
            duration.ToString("F2"),
            stepCount.ToString(),
            idleAccumulator.ToString("F2"),
            sprintAccumulator.ToString("F2"),
            pathLength.ToString("F2"),
            shortestPath.ToString("F2"),
            efficiency.ToString("F3")
        }));

        FlushSceneToDisk();
        sceneActive = false;
    }

    public void IncrementCounter(string counterName)
    {
        // Extendable if more counters are added later; steps is the only
        // one currently requested.
        if (counterName == "steps")
            stepCount++;
    }

    public void AddIdleTime(float deltaSeconds)
    {
        idleAccumulator += deltaSeconds;
    }

    public void AddSprintTime(float deltaSeconds)
    {
        sprintAccumulator += deltaSeconds;
    }

    public void LogEvent(string eventType, string payload = "")
    {
        eventRows.Add(string.Join(",", new[]
        {
            participantId,
            sessionId,
            currentScene,
            Time.time.ToString("F2"),
            eventType,
            EscapeCsv(payload)
        }));
    }

    public void LogPosition(Vector2 position)
    {
        LogPositionInternal(position);
    }

    // --- Internal helpers ---

    private void LogPositionInternal(Vector2 position)
    {
        positionSamples.Add(position);
        positionRows.Add(string.Join(",", new[]
        {
            participantId,
            sessionId,
            currentScene,
            Time.time.ToString("F2"),
            position.x.ToString("F2"),
            position.y.ToString("F2")
        }));
    }

    private float ComputePathLength()
    {
        float total = 0f;
        for (int i = 1; i < positionSamples.Count; i++)
            total += Vector2.Distance(positionSamples[i - 1], positionSamples[i]);
        return total;
    }

    /// <summary>
    /// Straight-line distance between first and last sample as a proxy for
    /// shortest path. Swap this out for a navmesh-based distance if a scene's
    /// geometry makes straight-line a poor approximation.
    /// </summary>
    private float ComputeShortestPath()
    {
        if (positionSamples.Count < 2) return 0f;
        return Vector2.Distance(positionSamples[0], positionSamples[positionSamples.Count - 1]);
    }

    private string EscapeCsv(string value)
    {
        if (string.IsNullOrEmpty(value)) return "";
        if (value.Contains(",") || value.Contains("\""))
            return "\"" + value.Replace("\"", "\"\"") + "\"";
        return value;
    }

    private string GetOutputDir()
    {
        string dir = Path.Combine(Application.persistentDataPath, outputFolder);
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);
        return dir;
    }

    private void FlushSceneToDisk()
    {
        string dir = GetOutputDir();

        AppendRows(Path.Combine(dir, $"{participantId}_scene_summary.csv"), SceneSummaryHeader, sceneSummaryRows);
        AppendRows(Path.Combine(dir, $"{participantId}_events.csv"), EventHeader, eventRows);
        AppendRows(Path.Combine(dir, $"{participantId}_positions.csv"), PositionHeader, positionRows);

        sceneSummaryRows.Clear();
        eventRows.Clear();
        positionRows.Clear();
    }

    private void AppendRows(string path, string header, List<string> rows)
    {
        if (rows.Count == 0) return;

        bool fileExists = File.Exists(path);
        using (var writer = new StreamWriter(path, append: true, encoding: Encoding.UTF8))
        {
            if (!fileExists)
                writer.WriteLine(header);
            foreach (var row in rows)
                writer.WriteLine(row);
        }
    }

    public void SetParticipant(string id, string relativeOutputFolder)
    {
        participantId = id;
        outputFolder = relativeOutputFolder;
        loggingEnabled = true;
        Debug.Log($"Participant set: {id}, output folder (relative): {outputFolder}");
    }

    void OnApplicationQuit()
    {
        if (sceneActive)
            SceneExit();
    }
}