using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "Alice/Enemy Data")]
public class EnemyData : ScriptableObject
{
    public string enemyName;
    public float moveSpeed;
    public GameObject prefab;
}
