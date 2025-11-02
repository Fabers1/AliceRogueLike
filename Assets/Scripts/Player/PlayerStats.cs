using System;
using System.Collections;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [HideInInspector]
    public int currentLevel = 1;

    public int curMaxHealth = 3;
    int originalMaxHealth;
    [HideInInspector]
    public int curHealth;

    [Header("Insanity Settings")]
    [Tooltip("Time in seconds before insanity effect triggers")]
    public float originalInsanityTriggerTime = 10f;
    float modifiedInsanityTriggerTimer;
    [Tooltip("Duration of the insanity effect")]
    public float insanityEffectDuration = 10f;
    [Tooltip("Maximum time that can be accumulated")]
    public float maxInsanityTime = 120f;

    private Coroutine insanityCoroutine;

    [HideInInspector]
    public float currentInsanityTimer;
    [HideInInspector]
    public bool insanityActive = false;

    [Header("Invincibility Settings")]
    public float invincibilityDuration = 1.5f;
    [HideInInspector]
    public bool isInvincible = false;
    
    Vector3 originalScale;

    public PlayerAnimation animations;

    public bool isDead = false;

    public AudioSource soundPlayer;
    public AudioClip aliceHurt;
    public AudioClip aliceDeath;

    public GameObject gameOverScreen;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        originalScale = transform.localScale;
    }

    private void Start()
    {
        if (GameManager.instance != null) 
        {
            curMaxHealth = GameManager.instance.startHealth;
            currentLevel = GameManager.instance.level;
        }

        curHealth = curMaxHealth;
        originalMaxHealth = curMaxHealth;
        modifiedInsanityTriggerTimer = originalInsanityTriggerTime;

        OnHealthChanged?.Invoke(curHealth);
        OnInsanityTimerChanged?.Invoke(currentInsanityTimer, modifiedInsanityTriggerTimer);
    }

    private void Update()
    {
        if (isDead || insanityActive) return;

        // Decrease timer over time
        currentInsanityTimer += Time.deltaTime;

        // Trigger insanity when timer reaches zero
        if (currentInsanityTimer >= modifiedInsanityTriggerTimer)
        {
            currentInsanityTimer = modifiedInsanityTriggerTimer;
            TriggerInsanity();
        }

        OnInsanityTimerChanged?.Invoke(currentInsanityTimer, modifiedInsanityTriggerTimer);
    }

    public void RecoverHealth(int health)
    {
        curHealth += health;

        if (curHealth > curMaxHealth)
        {
            curHealth = curMaxHealth;
        }

        OnHealthChanged?.Invoke(curHealth);
    }

    public void TakeDamage(int damage)
    {
        if (isDead || isInvincible) return;

        curHealth -= damage;
        animations.anim.SetTrigger("IsHurt");

        StartCoroutine(InvincibilityWindow());

        if (curHealth <= 0)
        {
            curHealth = 0;

            Death();

            OnHealthChanged?.Invoke(curHealth);

            return;
        }

        soundPlayer.PlayOneShot(aliceHurt);

        OnHealthChanged?.Invoke(curHealth);
    }

    IEnumerator InvincibilityWindow()
    {
        isInvincible = true;
        StartCoroutine(FlashPlayer());

        yield return new WaitForSeconds(invincibilityDuration);

        isInvincible = false;
    }

    IEnumerator FlashPlayer()
    {
        SpriteRenderer sprite = GetComponent<SpriteRenderer>();
        if (sprite == null) yield break;

        float flashInterval = 0.1f;
        float elapsed = 0f;

        while (elapsed < invincibilityDuration)
        {
            sprite.enabled = !sprite.enabled;
            yield return new WaitForSeconds(flashInterval);
            elapsed += flashInterval;
        }

        sprite.enabled = true;
    }

    public void OnLevelUp()
    {
        if(GameManager.instance == null) return;

        currentLevel = GameManager.instance.level;
        curMaxHealth = GameManager.instance.startHealth;
        originalMaxHealth = curMaxHealth;

        curHealth += currentLevel;
        if(curHealth > curMaxHealth)
        {
            curHealth = curMaxHealth;
        }

        OnHealthChanged?.Invoke(curHealth);
    }

    public void GainXP(int amount)
    {
        if(GameManager.instance != null)
        {
            int previousLevl = GameManager.instance.level;
            GameManager.instance.AddXP(amount);

            if(GameManager.instance.level > previousLevl)
            {
                OnLevelUp();
            }
        }
    }

    public void DelayInsanity(float amount)
    {
        if(isDead) return;

        currentInsanityTimer -= amount;

        if(currentInsanityTimer <= 0)
        {
            currentInsanityTimer = 0;
        }

        OnInsanityTimerChanged?.Invoke(currentInsanityTimer, modifiedInsanityTriggerTimer);
    }

    public void TriggerInsanity()
    {
        if (insanityActive || isDead) return;

        if(insanityCoroutine != null)
        {
            StopCoroutine(insanityCoroutine);
        }

        insanityCoroutine = StartCoroutine(InsanityEffect());
    }

    IEnumerator InsanityEffect()
    {
        insanityActive = true;
        OnInsanityStateChanged?.Invoke(true);

        int randomEffect = UnityEngine.Random.Range(0, 2);

        PlayerMovement movement = GetComponent<PlayerMovement>();
        float originalSpeed = movement != null ? movement.modifiedSpeed : 0f;
        insanityActive = true;

        if (randomEffect == 0)
        {
            transform.localScale = originalScale * 0.5f;

            curMaxHealth = Mathf.Max(1, originalMaxHealth / 2);
            if (curHealth > curMaxHealth)
            {
                curHealth = curMaxHealth;
                OnHealthChanged?.Invoke(curHealth);
            }

            if(movement != null)
            {
                movement.modifiedSpeed *= 2;
            }

            Debug.Log("Garrafa Misteriosa");
        }
        else if (randomEffect == 1)
        {
            transform.localScale = originalScale * 2f;

            curMaxHealth *= 2;

            curHealth *= 2;

            if (curHealth > curMaxHealth)
            {
                curHealth = curMaxHealth;
            }

            OnHealthChanged?.Invoke(curHealth);

            if(movement != null)
            {
                movement.modifiedSpeed = originalSpeed * 0.5f;
            }

            Debug.Log("Bolo Misterioso");
        }

        yield return new WaitForSeconds(insanityEffectDuration);

        Debug.Log("Time's Up!");

        transform.localScale = originalScale;
        curMaxHealth = originalMaxHealth;

        if(curHealth > curMaxHealth)
        {
            curHealth = curMaxHealth;
            OnHealthChanged?.Invoke(curHealth);
        }
        
        if(movement != null)
        {
            movement.modifiedSpeed = movement.originalSpeed;
        }

        currentInsanityTimer = 0;

        modifiedInsanityTriggerTimer = originalInsanityTriggerTime;
        insanityActive = false;

        OnInsanityStateChanged?.Invoke(false);
        OnInsanityTimerChanged?.Invoke(currentInsanityTimer, originalInsanityTriggerTime);
    }

    private void Death()
    {
        Debug.Log("Game Over!");

        isDead = true;

        gameOverScreen.SetActive(true);

        VictoryDefeatManager vdm = FindFirstObjectByType<VictoryDefeatManager>();
        if (vdm != null)
        {
            vdm.MostrarDerrota();
        }

        animations.anim.SetTrigger("dead");

        soundPlayer.PlayOneShot(aliceDeath);
        // Colocar o fim do jogo
    }

    public event Action<int> OnHealthChanged;
    public event Action<float, float> OnInsanityTimerChanged;
    public event Action<bool> OnInsanityStateChanged;
}