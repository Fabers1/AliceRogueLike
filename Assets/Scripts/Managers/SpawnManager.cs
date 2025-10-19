using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager instance;

    [SerializeField] List<EnemyPool> enemyPools = new List<EnemyPool>();

    [SerializeField] StageConfiguration currentStage;

    [SerializeField] bool showDebugInfo = true;

    public UnityEngine.Events.UnityEvent OnStageCompleted;
    public event System.Action<int, int> OnEnemyCountChanged;

    Dictionary<EnemyData, EnemyPool> poolLookup;
    List<Enemy> activeEnemies = new List<Enemy>();
    int enemiesSpawned = 0;
    int enemiesDefeated = 0;
    bool stageActive = false;
    Coroutine spawnCoroutine;

    private Boss currentBoss = null;

    private void Awake()
    {
        instance = this;

        InitializePoolLookup();
    }

    private void InitializePoolLookup()
    {
        poolLookup = new Dictionary<EnemyData, EnemyPool>();

        foreach (var pool in enemyPools)
        {
            EnemyData data = pool.GetEnemyData();
            if (data != null && !poolLookup.ContainsKey(data))
            {
                poolLookup.Add(pool.GetEnemyData(), pool);
            }
        }
    }

    public void StartStage(StageConfiguration stage)
    {
        if (stageActive)
        {
            Debug.LogWarning("Wave active!");
            return;
        }

        currentStage = stage;
        enemiesSpawned = 0;
        enemiesDefeated = 0;
        activeEnemies.Clear();
        stageActive = true;

        spawnCoroutine = StartCoroutine(SpawnCoroutine());
    }

    IEnumerator SpawnCoroutine()
    {
        yield return new WaitForSeconds(currentStage.initialSpawnDelay);

        while(enemiesSpawned < currentStage.totalEnemyCount)
        {
            if(activeEnemies.Count < currentStage.maxSimultaneousEnemies)
            {
                SpawnRandomEnemy();
            }

            yield return new WaitForSeconds(currentStage.spawnCheckInterval);
        }

        yield return new WaitUntil(() => activeEnemies.Count == 0);

        CompleteStage();
    }

    void SpawnRandomEnemy()
    {
        EnemyData enemyData = currentStage.GetRandomEnemyType();

        if(enemyData == null)
        {
            Debug.LogError("No enemy types configured in stage!");
            return;
        }

        if(!poolLookup.TryGetValue(enemyData, out EnemyPool pool))
        {
            Debug.LogError($"No pool found for enemyType: {enemyData.enemyName}");
            return;
        }

        Vector2 spawnPos = currentStage.GetRandomSpawnPoint();

        Enemy enemy = pool.SpawnEnemy(spawnPos);

        if(enemy == null)
        {
            Debug.LogError("Failed to spawn enemy from pool!");
            return;
        }

        activeEnemies.Add(enemy);
        enemiesSpawned++;

        enemy.OnDeath += HandleEnemyDeath;

        OnEnemyCountChanged?.Invoke(activeEnemies.Count, currentStage.totalEnemyCount - enemiesDefeated);

        if (showDebugInfo)
        {
            Debug.Log($"Spawned {enemyData.enemyName} ({enemiesSpawned}/{currentStage.totalEnemyCount}) - Active: {activeEnemies.Count}");
        }
    }

    void HandleEnemyDeath(Enemy enemy) 
    {
        enemy.OnDeath -= HandleEnemyDeath;

        activeEnemies.Remove(enemy);
        enemiesDefeated++;

        OnEnemyCountChanged?.Invoke(activeEnemies.Count, currentStage.totalEnemyCount - enemiesDefeated);

        if (showDebugInfo)
        {
            Debug.Log($"Enemy defeated! ({enemiesDefeated}/{currentStage.totalEnemyCount}) - Active: {activeEnemies.Count}");
        }

        // Check if stage is complete
        // This happens when all enemies spawned AND all are defeated
        if (enemiesSpawned >= currentStage.totalEnemyCount && activeEnemies.Count == 0)
        {
            CompleteStage();
        }
    }

    private void CompleteStage()
    {
        if (!stageActive) return;

        if(currentStage.hasBoss && currentBoss == null)
        {
            SpawnBoss();
        }
        else
        {
            stageActive = false;

            Debug.Log($"Stage Complete: {currentStage.stageName}");

            // Stop spawning coroutine if still running
            if (spawnCoroutine != null)
            {
                StopCoroutine(spawnCoroutine);
                spawnCoroutine = null;
            }

            // Invoke completion event
            OnStageCompleted?.Invoke();
        }
    }

    private void SpawnBoss()
    {
        Debug.Log("All enemies defeated! Spawning boss...");

        if(currentStage.stopEnemiesForBoss && spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }

        GameObject bossObj = Instantiate(
            currentStage.bossData.bossPrefab,
            currentStage.bossSpawnPosition,
            Quaternion.identity
        );

        currentBoss = bossObj.GetComponent<Boss>();
        if (currentBoss != null) 
        {
            currentBoss.Initialize(currentStage.bossData);
            currentBoss.OnBossDeath += HandleBossDeath;

            Debug.Log($"Boss spawned: {currentStage.bossData.bossName}");
        }
        else
        {
            Debug.LogError("Boss prefab missing Boss component");
        }
    }

    void HandleBossDeath(Boss boss)
    {
        Debug.Log("Boss defeated! Stage complete!");

        boss.OnBossDeath -= HandleBossDeath;
        currentBoss = null;

        stageActive = false;
        OnStageCompleted?.Invoke();
    }

    // Clear all enemies (for transitioning between stages)
    public void ClearStage()
    {
        // Stop spawning
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }

        // Return all active enemies to pools
        List<Enemy> enemiesToClear = new List<Enemy>(activeEnemies);
        foreach (var enemy in enemiesToClear)
        {
            enemy.OnDeath -= HandleEnemyDeath;
            enemy.ReturnToPool();
        }

        if(currentBoss != null)
        {
            currentBoss.OnBossDeath -= HandleBossDeath;
            Destroy(currentBoss.gameObject);
            currentBoss = null;
        }

        activeEnemies.Clear();
        stageActive = false;

        Debug.Log("Stage cleared");
    }

    // Public getters for UI/debugging
    public int GetEnemiesSpawned() => enemiesSpawned;
    public int GetEnemiesDefeated() => enemiesDefeated;
    public int GetActiveEnemyCount() => activeEnemies.Count;
    public int GetRemainingEnemies() => currentStage.totalEnemyCount - enemiesDefeated;
    public bool IsStageActive() => stageActive;
}
