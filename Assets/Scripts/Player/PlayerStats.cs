using System;
using System.Collections;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public int curMaxHealth = 3;
    int originalMaxHealth;
    [HideInInspector]
    public int curHealth;

    public int maxInsanity = 50;
    int curInsanity;
    public float insanityDelay = 10f;
    public bool insanityActive = false;

    public int xp;
    public int xpThreshold;
    public int level = 1;

    Vector3 originalScale;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        originalMaxHealth = curMaxHealth;
        curHealth = curMaxHealth;

        originalScale = transform.localScale;
    }

    public void RecoverHealth(int health)
    {
        curHealth += health;

        if(curHealth > curMaxHealth)
        {
            curHealth = curMaxHealth;
        }

        OnHealthChanged?.Invoke(curHealth, curMaxHealth);
    }

    public void TakeDamage(int damage)
    {
        curHealth -= damage;

        // Colocar lógica de invulnerabilidade

        Debug.Log("Hurt");

        if (curHealth <= 0)
        {
            Death();
        }

        OnHealthChanged?.Invoke(curHealth, curMaxHealth);
    }

    public void LevelUp()
    {
        level++;
        xpThreshold += 1000;
        xp = 0;
        curMaxHealth += level;
        originalMaxHealth += level;
        curHealth += level;
    }

    public void IncreaseInsanity()
    {
        curInsanity += 2;

        if(curInsanity >= maxInsanity)
        {
            StartCoroutine(InsanityOn());
        }
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

            GetComponent<PlayerMovement>().modifiedSpeed /=  2;

            Debug.Log("Bolo Misterioso");
        }

        yield return new WaitForSeconds(insanityDelay);

        Debug.Log("Time's Up!");

        transform.localScale = originalScale;
        curMaxHealth = originalMaxHealth;

        insanityActive = false;

        if(curHealth > curMaxHealth)
        {
            curHealth = curMaxHealth;
        }

        GetComponent<PlayerMovement>().modifiedSpeed = GetComponent<PlayerMovement>().originalSpeed;
    }

    private void Death()
    {
        Debug.Log("Game Over!");

        // Colocar o fim do jogo
    }

    public event System.Action<int, int> OnHealthChanged;
}
