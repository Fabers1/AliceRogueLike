using System.Collections.Generic;
using UnityEngine;

public enum SpawnLocationType
{
    PlatformCenter,
    PlatformEdge,
    Custom,
    Anywhere
}

[System.Serializable]
public class EnemySpawnSettings
{
    public EnemyData enemyType;

    [Header("Spawn Frequency")]
    [Tooltip("Quanto maior o valor, mais provável de spawnar")]
    [Range(1, 100)]
    public int spawnWeight = 10;

    [Tooltip("Delay mínimo para spawnar esse inimigo")]
    public float minSpawnDelay = 1f;

    [Tooltip("Delay máximo para spawnar esse inimigo")]
    public float maxSpawnDelay = 3f;

    [Header("Spawn Location Settings")]
    [Tooltip("Onde esse tipo de inimigo deveria spawnar?")]
    public SpawnLocationType spawnLocationType = SpawnLocationType.Anywhere;

    [Tooltip("Pontos de spawn customizaveis para esse inimigo (só é usado se o SpawnLocationType for Custom")]
    public List<Vector2> customSpawnPoints = new List<Vector2>();

    [Tooltip("Quão longe da beirada da plataforma vai spawnar em unidades de mundo (se o SpawnLocationType for PlatformEdge")]
    [Range(0.1f, 5f)]
    public float edgeOffset = 0.5f;

    [Tooltip("Quão longe da beirada da plataforma vai spawnar em unidades de mundo (se o SpawnLocationType for PlatformEdge")]
    [Range(0f, 5f)]
    public float centerRandomOffset = 1f;
}

[CreateAssetMenu(fileName = "StageConfig", menuName ="Alice/Stage Configuration")]
public class StageConfiguration : ScriptableObject
{
    [Header("Stage Identity")]
    public string stageName;
    public int stageNumber;

    [Header("Enemy Spawning")]
    public int totalEnemyCount = 50;

    public int maxSimultaneousEnemies = 10;

    public List<EnemySpawnSettings> enemyTypes = new List<EnemySpawnSettings>();

    public List<Vector2> spawnPoints = new List<Vector2>();

    public float initialSpawnDelay = 1f;

    public float spawnCheckInterval = 0.5f;

    [Header("Spawn Safety")]
    [Tooltip("Minimum distance from player to spawn enemies")]
    [Range(2f, 20f)]
    public float minDistanceFromPlayer = 5f;

    [Tooltip("Minimum distance between spawned enemies")]
    [Range(0.5f, 5f)]
    public float minDistanceBetweenEnemies = 2f;

    [Tooltip("Maximum attempts to find a safe spawn point")]
    [Range(5, 50)]
    public int maxSpawnAttempts = 20;

    public bool waitForCompletion = true;

    public float healthMultiplier = 1f;

    public float speedMultiplier = 1f;

    [Header("Boss Settings")]
    [Tooltip("Does this stage have a boss?")]
    public bool hasBoss = false;

    [Tooltip("Boss to spawn at end of stage")]
    public BossData bossData;

    [Tooltip("Spawn position for boss")]
    public Vector2 bossSpawnPosition = new Vector2(0, 5);

    [Tooltip("Should normal enemies stop spawning when boss appears?")]
    public bool stopEnemiesForBoss = true;

    public EnemyData GetRandomEnemyType()
    {
        if (enemyTypes.Count == 0) return null;

        // Calculate total weight
        int totalWeight = 0;
        foreach (var setting in enemyTypes)
        {
            totalWeight += setting.spawnWeight;
        }

        // Pick random value
        int randomValue = Random.Range(0, totalWeight);

        // Find which enemy this corresponds to
        int currentWeight = 0;
        foreach (var setting in enemyTypes)
        {
            currentWeight += setting.spawnWeight;
            if (randomValue < currentWeight)
            {
                return setting.enemyType;
            }
        }

        return enemyTypes[0].enemyType; // Fallback
    }

    public Vector2 GetRandomSpawnPoint()
    {
        if(spawnPoints.Count == 0)
        {
            Debug.LogWarning("No spawn points defined!");
            return Vector2.zero;
        }

        return spawnPoints[Random.Range(0, spawnPoints.Count)];
    }

    public float GetSpawnDelayForEnemy(EnemyData enemy)
    {
        foreach (var setting in enemyTypes)
        {
            if(setting.enemyType == enemy)
            {
                return Random.Range(setting.minSpawnDelay, setting.maxSpawnDelay);
            }
        }

        return 2f;
    }
}
