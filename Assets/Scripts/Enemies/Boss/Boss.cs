using UnityEngine;

public class Boss : MonoBehaviour
{
    public enum BossState
    {
        Idle,
        Moving,
        Invulnerable,
        Vulnerable,
        Dying
    }

    BossData data;
    BossState currentState = BossState.Idle;
    SpriteRenderer sr;
    Rigidbody2D rb;

    int curHealth;
    bool isInvulnerable = true;
    Transform player;
    float stateTimer = 0f;

    float nextBurstTime = 0f;
    bool isShooting = false;

    public System.Action<Boss> OnBossDeath;
    public System.Action<Boss, int> OnBossHealthChanged;
    public System.Action<Boss, BossState> OnBossStateChanged;

    private void Awake()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
