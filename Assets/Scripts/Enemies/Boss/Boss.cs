using System;
using System.Collections;
using Unity.VisualScripting;
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
        sr = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if(playerObj != null)
        {
            player = playerObj.transform;
        }
        else
        {
            Debug.LogError("Boss can't find player! Make sure player has 'Player' tag");
        }
    }

    public void Initialize(BossData bossData)
    {
        data = bossData;
        curHealth = data.maxHealth;
        isInvulnerable = data.startsInvulnerable;

        if(sr != null)
        {
            sr.color = isInvulnerable ? data.invulnerableColor : data.vulnerableColor;
        }

        ChangeState(BossState.Moving);
    }

    // Update is called once per frame
    void Update()
    {
        stateTimer += Time.deltaTime;

        switch (currentState)
        {
            case BossState.Idle:
                break;
            case BossState.Moving:
                UpdateMovingState();
                break;
            case BossState.Invulnerable:
                UpdateInvulnerableState();
                break;
            case BossState.Vulnerable:
                UpdateVulnerableState();
                break;
            case BossState.Dying:
                UpdateDyingState();
                break;
        }
    }

    void ChangeState(BossState newState)
    {
        ExitState(currentState);

        BossState oldState = currentState;
        currentState = newState;
        stateTimer = 0f;

        EnterState(newState);

        OnBossStateChanged?.Invoke(this, newState);

        Debug.Log($"Boss state: {oldState} > {newState}");
    }

    void EnterState(BossState state)
    {
        switch (state)
        {
            case BossState.Invulnerable:
                isInvulnerable = true;
                sr.color = data.invulnerableColor;
                nextBurstTime = Time.time + 0.5f; // Start shooting soon
                break;

            case BossState.Vulnerable:
                isInvulnerable = false;
                sr.color = data.vulnerableColor;
                StopAllCoroutines(); // Stop any shooting
                break;

            case BossState.Dying:
                StopAllCoroutines();
                break;
        }
    }

    void ExitState(BossState state)
    {
        switch (state)
        {
            case BossState.Moving:
                rb.linearVelocity = Vector2.zero;
                break;
        }
    }

    void UpdateMovingState()
    {
        if (player == null)
        {
            ChangeState(BossState.Idle);
            return;
        }

        isInvulnerable = true;

        // Target: directly above player, 3 units up
        Vector2 targetPosition = new Vector2(player.position.x, player.position.y + 2.5f);
        Vector2 currentPosition = transform.position;

        // Calculate direction to target
        Vector2 direction = (targetPosition - currentPosition).normalized;

        // Move toward target
        rb.linearVelocity = direction * data.moveSpeed * 2f; // Move faster during positioning

        // Check if we're close enough to target
        float distanceToTarget = Vector2.Distance(currentPosition, targetPosition);
        if (distanceToTarget < 0.5f)
        {
            // Reached position! Start attack pattern
            rb.linearVelocity = Vector2.zero;
            ChangeState(BossState.Invulnerable);
        }
    }

    void UpdateInvulnerableState()
    {
        if (player == null) return;

        // Follow player horizontally (stay above them)
        Vector2 targetX = new Vector2(player.position.x, transform.position.y);
        Vector2 currentPos = transform.position;
        Vector2 direction = (targetX - currentPos).normalized;

        // Smooth following movement
        rb.linearVelocity = new Vector2(direction.x * data.moveSpeed, 0);

        // Shoot projectiles periodically
        if (Time.time >= nextBurstTime && !isShooting)
        {
            StartCoroutine(ShootProjectileBurst());
        }

        // Check if invulnerable time is up
        if (stateTimer >= data.invulnerableTime)
        {
            ChangeState(BossState.Vulnerable);
        }
    }

    void UpdateVulnerableState()
    {
        rb.linearVelocity = Vector2.zero;

        if(stateTimer >= data.vulnerableTime)
        {
            ChangeState(BossState.Invulnerable);
        }
    }

    void UpdateDyingState()
    {
        // Death animation/effects happen here

        if (stateTimer >= 1f)
        {
            OnBossDeath?.Invoke(this);
            Destroy(gameObject);
        }
    }

    IEnumerator ShootProjectileBurst()
    {
        isShooting = true;

        for (int i = 0; i < data.projectilesPerBurst; i++)
        {
            ShootProjectile();

            yield return new WaitForSeconds(data.timeBetweenProjectiles);
        }

        nextBurstTime = Time.time + data.timeBetweenBursts;
        isShooting = false;
    }

    private void ShootProjectile()
    {
        if (data.projectilePrefab == null || player == null) return;

        // Spawn projectile at boss position
        GameObject projectileObj = Instantiate(data.projectilePrefab, transform.position, Quaternion.identity);

        // Get the projectile component
        BossProjectile projectile = projectileObj.GetComponent<BossProjectile>();
        if (projectile != null)
        {
            // Calculate direction to player (with some prediction)
            Vector2 direction = (player.position - transform.position).normalized;

            // Initialize projectile
            projectile.Initialize(direction, data.projectileSpeed);
        }
        else
        {
            Debug.LogError("Projectile prefab missing BossProjectile component!");
        }
    }

    public void TakeDamage(int damage)
    {
        if (isInvulnerable)
        {
            Debug.Log("Boss is invulnerable! No damage taken.");
            return;
        }

        curHealth -= damage;
        curHealth = Mathf.Max(0, curHealth);

        Debug.Log($"Boss took {damage} damage! Health: {curHealth}/{data.maxHealth}");

        EnterState(BossState.Invulnerable);

        isInvulnerable = true;

        OnBossHealthChanged?.Invoke(this, curHealth);

        if(curHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("Boss defeates!");

        ChangeState(BossState.Dying);
    }

    public bool IsInvulnerable() => isInvulnerable;
    public BossState GetCurrentState() => currentState;
    public int GetCurrentHealth() => curHealth;
    public int GetMaxHealth() => data.maxHealth;
    public float GetHealthPercentage() => (float)curHealth / data.maxHealth;
}