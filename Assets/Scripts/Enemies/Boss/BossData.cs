using UnityEngine;

/// <summary>
/// Defines all the data for a boss character.
/// Similar to EnemyData, but with special boss-specific properties.
/// </summary>
[CreateAssetMenu(fileName = "NewBoss", menuName = "Alice/Boss Data")]
public class BossData : ScriptableObject
{
    public string bossName = "Queen of Hearts";

    public int maxHealth = 20;

    public float moveSpeed = 0.8f;

    public bool startsInvulnerable = true;
    public float invulnerableTime = 5f;
    public float vulnerableTime = 3f;

    public GameObject projectilePrefab;
    public int projectilesPerBurst = 5;
    public float timeBetweenProjectiles = 0.3f;

    public float timeBetweenBursts = 3f;
    public float projectileSpeed = 5f;

    public GameObject bossPrefab;

    public Color invulnerableColor = new Color(1f, 0.5f, 0.5f, 1f);

    public Color vulnerableColor = Color.white;

    public bool completeStage = true;
}
