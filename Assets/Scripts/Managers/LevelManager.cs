using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SpawnManager spawnManager;

    [Header("Level Configuration")]
    [SerializeField] private List<StageConfiguration> stages;
    private int currentStageIndex = 0;

    [Header("UI Events")]
    public UnityEngine.Events.UnityEvent<string> OnStageStarted; // Stage name
    public UnityEngine.Events.UnityEvent<int> OnLevelComplete; // Final stage number

    public GameObject winScreen;

    private void Start()
    {
        if (spawnManager != null)
        {
            spawnManager.OnStageCompleted.AddListener(OnStageCompleted);
        }

        winScreen.SetActive(false);

        StartNextStage();
    }

    private void OnStageCompleted()
    {
        Debug.Log($"Stage {currentStageIndex + 1} completed!");

        // Move to next stage
        currentStageIndex++;

        if (currentStageIndex < stages.Count)
        {
            // Start next stage after a brief delay
            Invoke(nameof(StartNextStage), 2f);
        }
        else
        {
            OnLevelComplete?.Invoke(stages.Count);

            winScreen.SetActive(true);
            Debug.Log("All stages complete!");
        }
    }

    private void StartNextStage()
    {
        if (currentStageIndex >= stages.Count)
        {
            Debug.LogWarning("No more stages available!");
            return;
        }

        StageConfiguration stage = stages[currentStageIndex];
        Debug.Log($"Starting Stage {currentStageIndex + 1}: {stage.stageName}");

        OnStageStarted?.Invoke(stage.stageName);
        spawnManager.StartStage(stage);
    }

    public void OnPowerUpSelectedContinue()
    {
        if (currentStageIndex < stages.Count)
        {
            StartNextStage();
        }
        else
        {
            Debug.Log("All stages complete!");
        }
    }

    // Manual controls for testing
    public void RestartCurrentStage()
    {
        spawnManager.ClearStage();
        spawnManager.StartStage(stages[currentStageIndex]);
    }

    public void SkipToNextStage()
    {
        spawnManager.ClearStage();
        OnStageCompleted();
    }

    public void RestartLevel()
    {
        spawnManager.ClearStage();
        currentStageIndex = 0;
        StartNextStage();
    }
}
