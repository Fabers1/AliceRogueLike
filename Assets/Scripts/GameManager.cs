using UnityEngine;

public class GameManager : MonoBehaviour
{
    public SpawnManager spawnManager;
    public StageConfiguration currentWave;
    public float startDelay = 0.5f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Invoke("StartSpawn", startDelay);
    }

    public void WinGame()
    {
        Debug.Log("Level Won!");
    }
}
