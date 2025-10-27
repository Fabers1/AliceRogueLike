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
    public int maxInsanity = 50;
    [HideInInspector]
    public int curInsanity;
    public float insanityDelay = 10f;
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

        OnHealthChanged?.Invoke(curHealth);
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

        soundPlayer.PlayOneShot(aliceHurt);

        if (curHealth <= 0)
        {
            curHealth = 0;

            Death();
        }

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

    public void IncreaseInsanity()
    {
        curInsanity += 2;

        if (curInsanity >= maxInsanity)
        {
            StartCoroutine(InsanityOn());
        }

        OnInsanityChanged?.Invoke(curInsanity);
    }

    IEnumerator InsanityOn()
    {
        int randomEffect = UnityEngine.Random.Range(0, 2);

        insanityActive = true;

        if (randomEffect == 0)
        {
            Vector3 currentScale = transform.localScale;
            currentScale /= 2;
            transform.localScale = currentScale;

            curMaxHealth /= 2;
            if (curHealth > curMaxHealth)
            {
                curHealth = curMaxHealth;
            }

            GetComponent<PlayerMovement>().modifiedSpeed *= 2;

            Debug.Log("Garrafa Misteriosa");
        }
        else if (randomEffect == 1)
        {
            Vector3 currentScale = transform.localScale;
            currentScale *= 2;
            transform.localScale = currentScale;

            curMaxHealth *= 2;

            curHealth *= 2;

            if (curHealth > curMaxHealth)
            {
                curHealth = curMaxHealth;
            }

            GetComponent<PlayerMovement>().modifiedSpeed /= 2;

            Debug.Log("Bolo Misterioso");
        }

        yield return new WaitForSeconds(insanityDelay);

        Debug.Log("Time's Up!");

        transform.localScale = originalScale;
        curMaxHealth = originalMaxHealth;

        curInsanity = 0;

        insanityActive = false;

        if (curHealth > curMaxHealth)
        {
            curHealth = curMaxHealth;
        }

        GetComponent<PlayerMovement>().modifiedSpeed = GetComponent<PlayerMovement>().originalSpeed;
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

    public event System.Action<int> OnHealthChanged;
    public event System.Action<int> OnInsanityChanged;
}