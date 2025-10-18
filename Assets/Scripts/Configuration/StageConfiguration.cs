using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemySpawnSettings
{
    public EnemyData enemyType;
    [Range(1, 100)]
    public int spawnWeight = 10;

    public float minSpawnDelay = 1f;

    public float maxSpawnDelay = 3f;
}

[CreateAssetMenu(fileName = "StageConfig", menuName ="Alice/Stage Configuration")]
public class StageConfiguration : ScriptableObject
{
    public string stageName;
    public int stageNumber;

    public int totalEnemyCount = 50;

    public int maxSimultaneousEnemies = 10;

    public List<EnemySpawnSettings> enemyTypes = new List<EnemySpawnSettings>();

    public List<Vector2> spawnPoints = new List<Vector2>();

    public float initialSpawnDelay = 1f;

    public float spawnCheckInterval = 0.5f;

    public bool waitForCompletion = true;

    public float healthMultiplier = 1f;

    public float speedMultiplier = 1f;

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
