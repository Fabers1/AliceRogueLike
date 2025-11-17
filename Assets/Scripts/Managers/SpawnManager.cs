using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager instance;

    [SerializeField] List<EnemyPool> enemyPools = new List<EnemyPool>();

    [SerializeField] Transform playerTransform;

    [SerializeField] StageConfiguration currentStage;

    [SerializeField] bool showDebugInfo = true;

    public UnityEngine.Events.UnityEvent OnStageCompleted;
    public event System.Action<int, int> OnEnemyCountChanged;

    Dictionary<EnemyData, EnemyPool> poolLookup;
    List<Enemy> activeEnemies = new List<Enemy>();
    int enemiesSpawned = 0;
    int enemiesDefeated = 0;
    int failedSpawnAttempts = 0;
    float originalDistanceToPlayer;
    bool stageActive = false;
    Coroutine spawnCoroutine;

    private Boss currentBoss = null;

    public AudioSource bossMusic;
    public AudioSource gameMusic;

    private void Awake()
    {
        instance = this;

        InitializePoolLookup();

        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) 
            { 
                playerTransform = player.transform;
            }
        }
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

        originalDistanceToPlayer = currentStage.minDistanceFromPlayer;

        spawnCoroutine = StartCoroutine(SpawnCoroutine());
    }

    IEnumerator SpawnCoroutine()
    {
        yield return new WaitForSeconds(currentStage.initialSpawnDelay);

        while (enemiesSpawned < currentStage.totalEnemyCount)
        {
            if (activeEnemies.Count < currentStage.maxSimultaneousEnemies)
            {
                TrySpawnRandomEnemy();
            }

            yield return new WaitForSeconds(currentStage.spawnCheckInterval);
        }

        yield return new WaitUntil(() => activeEnemies.Count == 0);

        CompleteStage();
    }

    void TrySpawnRandomEnemy()
    {
        EnemyData enemyData = currentStage.GetRandomEnemyType();

        if (enemyData == null)
        {
            Debug.LogError("No enemy types configured in stage!");
            return;
        }

        if (!poolLookup.TryGetValue(enemyData, out EnemyPool pool))
        {
            Debug.LogError($"No pool found for enemyType: {enemyData.enemyName}");
            return;
        }

        Vector2 safePos;
        if(FindSafeSpawnPosition(out safePos))
        {
            SpawnEnemyAt(enemyData, pool, safePos);
            failedSpawnAttempts = 0;
            currentStage.minDistanceFromPlayer = originalDistanceToPlayer;
        }
        else
        {
            failedSpawnAttempts++;

            Debug.LogWarning("Failed to find safe location");

            if(failedSpawnAttempts > 10)
            {
                currentStage.minDistanceFromPlayer -= 0.5f;
            }
        }
    }

    private bool FindSafeSpawnPosition(out Vector2 safePos)
    {
        safePos = Vector2.zero;

        if(playerTransform == null)
        {
            Debug.LogError("Player reference missing!");
            return false;
        }

        Vector2 playerPos = playerTransform.position;

        // Tenta encontrar uma posição segura
        for(int attempt = 0; attempt < currentStage.maxSpawnAttempts; attempt++)
        {
            Vector2 candidatePos = currentStage.GetRandomSpawnPoint();

            // Verifique distancia até o jogador
            float distanceToPlayer = Vector2.Distance(candidatePos, playerPos);
            if(distanceToPlayer < currentStage.minDistanceFromPlayer)
            {
                continue; // Perto demais do jogador, tente novamente
            }

            // Verifique a distância com outros inimigos ativos
            bool tooCloseToEnemies = false;
            foreach (Enemy enemy in activeEnemies)
            {
                float distanceToEnemy = Vector2.Distance(candidatePos, enemy.transform.position);
                if (distanceToEnemy < currentStage.minDistanceFromPlayer) 
                { 
                    tooCloseToEnemies = true;
                    break;
                }
            }

            if (tooCloseToEnemies) 
            {
                continue;
            }

            safePos = candidatePos;
            return true;
        }

        return false;
    }

    private void SpawnEnemyAt(EnemyData enemyData, EnemyPool pool, Vector2 safePos)
    {
        Enemy enemy = pool.SpawnEnemy(safePos);

        if (enemy == null)
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

        if (currentStage.hasBoss && currentBoss == null)
        {
            gameMusic.Pause();
            bossMusic.Play();

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

        if (currentStage.stopEnemiesForBoss && spawnCoroutine != null)
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

        if (currentBoss != null)
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
