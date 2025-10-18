using System;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int curMaxHealth = 3;
    int originalMaxHealth;
    [HideInInspector]
    public int curHealth;

    public int xp;
    public int xpThreshold;
    public int level = 1;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        originalMaxHealth = curMaxHealth;
        curHealth = curMaxHealth;
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
        curMaxHealth += level;
        curHealth += level;
    }

    private void Death()
    {
        Debug.Log("Game Over!");

        // Colocar o fim do jogo
    }

    public event System.Action<int, int> OnHealthChanged;
}
