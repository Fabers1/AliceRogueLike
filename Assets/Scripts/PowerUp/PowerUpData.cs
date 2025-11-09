using UnityEngine;

[CreateAssetMenu(fileName = "NewPowerUp", menuName = "Alice/PowerUp")]
public class PowerUpData : ScriptableObject
{
    public string powerUpName;
    [TextArea(3, 5)]
    public string description;
    public Sprite icon;
    public PowerUpType type;
    public int maxLevel = 3;

    [Header("Stats Modifiers")]
    public float damageMultiplier = 1f;
    public int healthIncrease = 0;
    public float speedMultiplier = 1f;
    public float attackRangeMultiplier = 1f;
    public float insanityDelayBonus = 0f;
    public int xpMultiplier = 1;

    [Header("Special Effects")]
    public bool duplicatesEnemyXP = false;
    public bool chooseNextTransformation = false;
}

public enum PowerUpType
{
    PirulitoCha,    // Aumenta o colisor
    CartaEspadas,   // Duplica o XP dos inimigos
    BiscoitoLirio,  // Escolhe a próxima transformação
    MovementSpeed,  // Boost de velocidade
    HealthBoost,    // Aumento de vida
    InsanityDelay   // Delay do tempo de insanidade
}