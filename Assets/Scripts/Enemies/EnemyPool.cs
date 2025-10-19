using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class EnemyPool : MonoBehaviour
{
    [SerializeField] EnemyData enemyData;
    [SerializeField] int initialPoolSize = 10;
    [SerializeField] int maxPoolSize = 50;

    ObjectPool<Enemy> pool;

    List<Enemy> activeEnemies = new List<Enemy>();

    private void Awake()
    {
        InitializePool();
    }

    public EnemyData GetEnemyData()
    {
        return enemyData;
    }

    void InitializePool()
    {
        pool = new ObjectPool<Enemy>(
            createFunc: CreateEnemy,
            actionOnGet: OnGetEnemy,
            actionOnRelease: OnReleaseEnemy,
            actionOnDestroy: OnDestroyEnemy,
            collectionCheck: false,
            defaultCapacity: initialPoolSize,
            maxSize: maxPoolSize
        );

        List<Enemy> tempList = new List<Enemy>();
        for (int i = 0; i < initialPoolSize; i++)
        {
            tempList.Add(pool.Get());
        }

        foreach (var enemy in tempList)
        {
            pool.Release(enemy);
        }
    }

    Enemy CreateEnemy()
    {
        GameObject enemyObj = Instantiate(enemyData.prefab);
        Enemy enemy = enemyObj.GetComponent<Enemy>();

        if(enemy == null)
        {
            Debug.LogError("Enemy prefab missing Enemy component");
            enemy = enemyObj.AddComponent<Enemy>();
        }

        return enemy;
    }

    void OnGetEnemy(Enemy enemy)
    {
        enemy.gameObject.SetActive(true);
        enemy.Initialize(enemyData, this);
        activeEnemies.Add(enemy);
    }

    void OnReleaseEnemy(Enemy enemy)
    {
        enemy.gameObject.SetActive(false);
        activeEnemies.Remove(enemy);
    }

    // Chama quando a pool estiver cheia
    void OnDestroyEnemy(Enemy enemy)
    {
        activeEnemies.Remove(enemy);
        Destroy(enemy.gameObject);
    }

    public Enemy SpawnEnemy(Vector2 pos)
    {
        Enemy enemy = pool.Get();
        enemy.transform.position = pos;
        return enemy;
    }

    public void ReturnEnemy(Enemy enemy)
    {
        pool.Release(enemy);
    }

    public void ReturnAllEnemies()
    {
        List<Enemy> enemiesToReturn = new List<Enemy>(activeEnemies);

        foreach(var enemy in enemiesToReturn)
        {
            pool.Release(enemy);
        }
    }

    public int GetActiveEnemyCount() => activeEnemies.Count;
}
